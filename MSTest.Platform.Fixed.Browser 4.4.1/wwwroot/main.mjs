import { dotnet } from './_framework/dotnet.js';

const { runMain } = await dotnet.withApplicationArgumentsFromQuery().create();

globalThis.mtpExitCode = await runMain();
