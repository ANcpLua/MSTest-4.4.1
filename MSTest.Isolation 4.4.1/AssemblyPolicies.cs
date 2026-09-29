// Every test is scheduled on its own, so a test takes its resource locks when it starts and releases them when
// it ends.
[assembly: Parallelize(Workers = 4, Scope = ExecutionScope.MethodLevel)]
