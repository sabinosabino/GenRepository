using System.Text;
using GenerateRepository;
// See https://aka.ms/new-console-template for more information
Console.WriteLine("Informe o nome do Modelo");

var result = Console.ReadLine();

var c = new GenFiles(result);

c.GenAllFiles();

Console.WriteLine("Finalizado...");
