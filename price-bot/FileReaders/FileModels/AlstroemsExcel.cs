using price_bot.Models;

namespace price_bot.FileReaders.FileModels;
public class AlstroemsExcel
{
    public required string Nummer { get; set; }
    public required string Beskrivelse { get; set; }
    public required string Enhedspris { get; set; }
    public required int Lager { get; set; }

    public AlstroemsProduct Convert()
    {
        return new AlstroemsProduct
        {
            ProductNumber = Nummer,
            ProductName = Beskrivelse,
            Price = Double.Parse(String.Join("", Enhedspris.Split(',')).Replace('.', ',')),
            Stock = Lager
        };
    }
}