// The tests read the dependency file, the output folder and the loaded assemblies and change none of them.
[assembly: Parallelize(Workers = 4, Scope = ExecutionScope.MethodLevel)]
