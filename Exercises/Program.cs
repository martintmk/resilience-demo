using Exercises;

var cancellationToken = CancellationToken.None;
var files = Enumerable.Range(0, 50).Select(v => $"file{v}.txt").ToArray();

await new Exercise1().Run(files, cancellationToken);
