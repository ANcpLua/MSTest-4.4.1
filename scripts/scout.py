#!/usr/bin/env python3
"""Scouts a newer MSTest release: what it adds to the public API, the analyzer rules and the changelog.

Reads the pinned version from global.json, takes the newest stable release on NuGet whose patch number
reaches the minimum, and compares the two tags of microsoft/testfx. Writes delta/<version>/ and changes
nothing else: the examples are updated by whoever picks the delta up.
"""
import argparse
import json
import os
import re
import subprocess
import sys
import tempfile
import urllib.request
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
INDEX = "https://api.nuget.org/v3-flatcontainer/mstest.sdk/index.json"
SOURCE = "https://github.com/microsoft/testfx.git"
# The changelogs are read from main: entries are still added to a release's section after its tag.
CHANGELOGS = "https://raw.githubusercontent.com/microsoft/testfx/main/docs/"
STABLE = re.compile(r"^\d+\.\d+\.\d+$")
SECTION = re.compile(r"^## .*\[(\d+\.\d+\.\d+)\]")
RULE = re.compile(r"^[A-Z]+\d+ \|")


def number(version):
    return tuple(int(part) for part in version.split("."))


def pinned():
    return json.loads((ROOT / "global.json").read_text())["msbuild-sdks"]["MSTest.Sdk"]


def download(url):
    with urllib.request.urlopen(url) as response:
        return response.read().decode("utf-8-sig")


def newest(minimum_patch):
    versions = json.loads(download(INDEX))["versions"]
    stable = [v for v in versions if STABLE.match(v) and number(v)[2] >= minimum_patch]
    return max(stable, key=number)


def checkout(version, directory):
    clone = subprocess.run(
        ["git", "-c", "advice.detachedHead=false", "clone", "--quiet", "--depth", "1",
         "--branch", "v" + version, SOURCE, str(directory)],
        stderr=subprocess.PIPE, text=True)
    if clone.returncode != 0:
        sys.exit("Tag v{} of microsoft/testfx could not be cloned:\n{}".format(version, clone.stderr))
    return directory


def platform(tree):
    text = (tree / "eng" / "Versions.props").read_text(encoding="utf-8-sig")
    return re.search(r"<TestingPlatformVersionPrefix>([^<]+)<", text).group(1)


def surface(tree):
    """The public API per PublicAPI folder: shipped and unshipped lines together, removals taken out."""
    folders = {}
    for file in sorted((tree / "src").rglob("PublicAPI.*.txt")):
        lines = file.read_text(encoding="utf-8-sig").splitlines()
        entry = folders.setdefault(str(file.parent.relative_to(tree / "src")), (set(), set()))
        for line in lines:
            if line.startswith("*REMOVED*"):
                entry[1].add(line[len("*REMOVED*"):])
            elif line.strip() and not line.startswith("#"):
                entry[0].add(line)
    return {folder: present - removed for folder, (present, removed) in folders.items()}


def rules(tree):
    """Analyzer release rows as (file, release, table, row)."""
    rows = set()
    for file in sorted((tree / "src").rglob("AnalyzerReleases.*.md")):
        release = table = ""
        for line in file.read_text(encoding="utf-8-sig").splitlines():
            if line.startswith("## "):
                release = line[3:].strip()
            elif line.startswith("### "):
                table = line[4:].strip()
            elif RULE.match(line):
                rows.add((str(file.parent.relative_to(tree / "src")), release, table, line.strip()))
    return rows


def sections(changelog, after, up_to):
    """The changelog sections of the versions above `after` and up to `up_to`, newest first."""
    kept, keeping = [], False
    for line in changelog.splitlines():
        heading = SECTION.match(line)
        if heading:
            keeping = number(after) < number(heading.group(1)) <= number(up_to)
        elif line.startswith("## "):
            keeping = False
        if keeping:
            kept.append(line)
    return "\n".join(kept).strip()


def api_report(old, new, before, after):
    added = {folder: sorted(after[folder] - before.get(folder, set())) for folder in after}
    removed = {folder: sorted(before[folder] - after.get(folder, set())) for folder in before}
    folders = sorted(f for f in set(added) | set(removed) if added.get(f) or removed.get(f))
    lines = ["| Surface | Added | Removed |", "|---|---:|---:|"]
    lines += ["| `{}` | {} | {} |".format(f, len(added.get(f, [])), len(removed.get(f, []))) for f in folders]
    for folder in folders:
        lines += ["", "## `{}`".format(folder)]
        for title, entries in (("Added", added.get(folder, [])), ("Removed", removed.get(folder, []))):
            if entries:
                lines += ["", "### {} ({})".format(title, len(entries)), "", "```text"] + entries + ["```"]
    totals = (sum(len(v) for v in added.values()), sum(len(v) for v in removed.values()))
    return "\n".join(lines), totals


def rule_report(before, after):
    fresh = sorted(after - before, key=lambda row: (row[0], [-n for n in release_number(row[1])], row[2], row[3]))
    lines, last = [], None
    for folder, release, table, row in fresh:
        if (folder, release, table) != last:
            lines += ["", "### `{}`, {}, {}".format(folder, release, table), "", "| Rule | | | |", "|---|---|---|---|"]
            last = (folder, release, table)
        lines.append("| " + row + " |")
    return "\n".join(lines).strip(), len(fresh)


def release_number(release):
    found = re.search(r"(\d+)\.(\d+)\.(\d+)", release)
    return [int(part) for part in found.groups()] if found else [0, 0, 0]


def write(directory, name, text):
    directory.mkdir(parents=True, exist_ok=True)
    (directory / name).write_text(text.rstrip() + "\n", encoding="utf-8")


def main():
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument("--from", dest="old", help="version to compare from; default: the pin in global.json")
    parser.add_argument("--to", dest="new", help="version to scout; default: the newest stable release on NuGet")
    parser.add_argument("--minimum-patch", type=int, default=1,
                        help="smallest patch number that counts as a release to scout (default 1: x.y.0 is skipped)")
    parser.add_argument("--output", help="folder to write; default: delta/<version> in the repository")
    arguments = parser.parse_args()

    old = arguments.old or pinned()
    new = arguments.new or newest(arguments.minimum_patch)
    if number(new) <= number(old):
        print("MSTest {} is pinned and nothing newer counts: newest is {}.".format(old, new))
        return 0

    with tempfile.TemporaryDirectory() as scratch:
        before = checkout(old, Path(scratch) / old)
        after = checkout(new, Path(scratch) / new)
        platforms = (platform(before), platform(after))
        api, (added, removed) = api_report(old, new, surface(before), surface(after))
        rule_rows, rule_count = rule_report(rules(before), rules(after))
    framework_log = sections(download(CHANGELOGS + "Changelog.md"), old, new)
    platform_log = sections(download(CHANGELOGS + "Changelog-Platform.md"), platforms[0], platforms[1])

    entries = sum(log.count("\n* ") + log.startswith("* ") for log in (framework_log, platform_log))
    title = "MSTest {} → {}".format(old, new)
    origin = ("Microsoft.Testing.Platform {} → {}. Source: [microsoft/testfx](https://github.com/microsoft/testfx), "
              "tags `v{}` and `v{}`; the changelogs come from `main`.").format(platforms[0], platforms[1], old, new)
    directory = Path(arguments.output) if arguments.output else ROOT / "delta" / new

    write(directory, "api.md", "\n".join([
        "# Public API: " + title, "", origin,
        "Taken from `PublicAPI.Shipped.txt` and `PublicAPI.Unshipped.txt` of every assembly.", "", api]))
    write(directory, "analyzers.md", "\n".join([
        "# Analyzer rules: " + title, "", origin,
        "Rows of `AnalyzerReleases.*.md` that the older tag does not have.", "", rule_rows or "No new rows."]))
    write(directory, "changelog.md", "\n".join([
        "# Changelog: " + title, "", origin, "", "# MSTest", "", framework_log or "No section.", "",
        "# Microsoft.Testing.Platform", "", platform_log or "No section."]))
    write(directory, "summary.md", "\n".join([
        "MSTest **{}** is released. This repository shows **{}**.".format(new, old), "",
        "| | |", "|---|---:|",
        "| Public API lines added | {} |".format(added),
        "| Public API lines removed | {} |".format(removed),
        "| Analyzer release rows added | {} |".format(rule_count),
        "| Changelog entries | {} |".format(entries), "",
        "- `delta/{}/api.md`: added and removed public API per assembly".format(new),
        "- `delta/{}/analyzers.md`: new and changed analyzer rules".format(new),
        "- `delta/{}/changelog.md`: the changelog sections of both packages".format(new), "",
        "This pull request only scouts. It changes no pin and no example."]))

    if os.environ.get("GITHUB_OUTPUT"):
        with open(os.environ["GITHUB_OUTPUT"], "a", encoding="utf-8") as output:
            output.write("from={}\nto={}\n".format(old, new))
    print("{}: {} API lines added, {} removed, {} analyzer rows, {} changelog entries. Written to {}".format(
        title, added, removed, rule_count, entries, directory))
    return 0


if __name__ == "__main__":
    sys.exit(main())
