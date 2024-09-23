# Magic Web Scrapper

Esta aplicação em C# faz a raspagem de dados de cartas de um site específico e salva as informações em um arquivo `.xls`.

## Funcionalidades

- Carrega a página HTML contendo a lista de links de cartas.
- Seleciona todos os links das cartas.
- Usa `Parallel.ForEach` para processar cada link:
    - Carrega a página HTML da carta.
    - Verifica se a página não é "Page Not Found".
    - Extrai o custo e a descrição da carta.
    - Adiciona um novo `CardRecord` à lista de cartas.
- Cria um `StringBuilder` para armazenar os dados das cartas.
- Escreve os dados das cartas em um arquivo `.xls`.

## Estrutura do Projeto

- `CP1/CardRecord.cs`: Define a estrutura do registro de cartas.
- `CP1/Program.cs`: Contém a lógica principal da aplicação.
- `CP1/readme.md`: Este arquivo de documentação.

## Como Executar

1. Clone o repositório.
2. Abra o projeto no seu IDE preferido (recomendado: JetBrains Rider).
3. Restaure os pacotes NuGet necessários.
4. Compile e execute o projeto.

## Dependências

- HtmlAgilityPack
- System.Text
- System.Text.RegularExpressions
- System.Web

## Saída

A aplicação gera um arquivo `cartas.xls` contendo as seguintes colunas:
- Nome
- Descrição
- Custo

O arquivo gerado pode ser encontrado na pasta `bin` do projeto.

## Exemplo de Uso

```csharp
using HtmlAgilityPack;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using CP1;

List<CardRecord> cartas = new List<CardRecord>();
var htmlListCard = new HtmlWeb().Load("https://sw-unlimited-db.com/cards/");
var listLinks = htmlListCard.DocumentNode.SelectNodes("//div[@class='relative']/a");

Parallel.ForEach(listLinks, link =>
{
    string? linkPagina = link.GetAttributes().ToList().FirstOrDefault()?.Value;
    var htmlCard = new HtmlWeb().Load($"https://sw-unlimited-db.com/cards/{linkPagina}");
    var tituloPagina = htmlCard.DocumentNode.SelectSingleNode("//h1");
    if (tituloPagina.InnerText != "Page Not Found")
    {
        var descricaoCarta = htmlCard.DocumentNode.SelectNodes("//div[@class='card-description']");
        var custoCarta = htmlCard.DocumentNode.SelectSingleNode("//*[text()='Cost:']")?.NextSibling?.InnerText.Trim();
        string descricao = descricaoCarta != null ? string.Join(" ", descricaoCarta.Select(x => x.InnerText).ToArray()) : null;
        short? custo = short.TryParse(custoCarta, out short parsedCusto) ? parsedCusto : (short?)null;

        cartas.Add(new CardRecord(
            HttpUtility.HtmlDecode(tituloPagina.InnerText),
            HttpUtility.HtmlDecode(descricao),
            custo)
        );
    }
});

StringBuilder arquivo = new StringBuilder();
arquivo.AppendLine("Nome;Descrição;Custo");
cartas.ForEach(carta => arquivo.AppendLine($"{carta.nome};{carta.descricao};{carta.custo}"));
File.WriteAllText("cartas.xls", arquivo.ToString(), UTF8Encoding.UTF8);
Console.WriteLine("Arquivo xls gerado com sucesso!");