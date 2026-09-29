// MSTEST0081 reports a provider that is given null instead of a filter type. In a test application every test
// of the assembly fails with UTA073, so this assembly is a test library that is built and never run.
#pragma warning disable MSTEST0081
[assembly: TestFilterProvider(null!)]
#pragma warning restore MSTEST0081
