using HtmlAgilityPack;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using CP1;

List<CardRecord> cartas = new List<CardRecord>();
var htmlListCard = new HtmlWeb().Load("https://sw-unlimited-db.com/cards/");
var listLinks = htmlListCard.DocumentNode.SelectNodes("/html/body/div[1]/div[2]/div/div/div[1]/div[1]/div[@class='relative']/a[1]");

Parallel.ForEach(listLinks, link =>
{
    string? linkPagina = link.GetAttributes().ToList().FirstOrDefault()?.Value;
    var htmlCard = new HtmlWeb().Load($"https://sw-unlimited-db.com/cards/{linkPagina}");
    var tituloPagina = htmlCard.DocumentNode.SelectSingleNode("//h1");
    if (tituloPagina.InnerText != "Page Not Found")
    {
        var descricaoCarta = htmlCard.DocumentNode.SelectNodes("/html/body/div/div/div/div[2]/div[1]/div/div[3]/p[1]");
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
File.WriteAllText("cartas.xls", arquivo.ToString());
Console.WriteLine("Arquivo gerado com sucesso!");