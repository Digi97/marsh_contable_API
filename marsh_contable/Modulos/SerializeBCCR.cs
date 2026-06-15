
using System.Collections.Generic;

public class BccrResponse
{
    public bool estado { get; set; }
    public string mensaje { get; set; }
    public List<BccrDato> datos { get; set; }
}

public class BccrDato
{
    public string titulo { get; set; }
    public string periodicidad { get; set; }
    public List<BccrIndicador> indicadores { get; set; }
}

public class BccrIndicador
{
    public string codigoIndicador { get; set; }
    public string nombreIndicador { get; set; }
    public List<BccrSerie> series { get; set; }
}

public class BccrSerie
{
    public string fecha { get; set; }
    public double valorDatoPorPeriodo { get; set; }
}