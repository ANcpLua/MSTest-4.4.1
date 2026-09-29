// The tests read the output folder and change nothing in it.
[assembly: Parallelize(Workers = 4, Scope = ExecutionScope.MethodLevel)]
