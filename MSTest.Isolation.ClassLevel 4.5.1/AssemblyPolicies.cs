// A class is scheduled as a whole: its tests run one after the other, and the class takes the resource locks of
// all its tests before the first of them starts.
[assembly: Parallelize(Workers = 4, Scope = ExecutionScope.ClassLevel)]
