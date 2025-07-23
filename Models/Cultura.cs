using System.Globalization;

public static class ConfiguracionGlobal
{
    /// <summary>
    /// Define una cultura estándar para español de Argentina, disponible en todo el sistema.
    /// Se usa para dar formato a números (ej: 1.000,00) y monedas.
    /// </summary>
    public static readonly CultureInfo CulturaES = new CultureInfo("es-AR");
    public static readonly CultureInfo CulturaUS = new CultureInfo("en-US");
}