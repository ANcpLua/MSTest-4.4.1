// Every test is scheduled on its own, so tests that share a prerequisite can run at the same time.
[assembly: Parallelize(Workers = 4, Scope = ExecutionScope.MethodLevel)]
