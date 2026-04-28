using Concesionario.Models;

public class IntentDetector
{
    public ChatIntent Detectar(string mensaje)
    {
        mensaje = mensaje.ToLower();

        if (mensaje.Contains("no me interesa") || mensaje.Contains("no me gusto") || mensaje.Contains("no"))
            return ChatIntent.Rechazo;

        if (mensaje.Contains("todos") || mensaje.Contains("mas") || mensaje.Contains("más"))
            return ChatIntent.VerMas;

        if (string.IsNullOrWhiteSpace(mensaje))
            return ChatIntent.Desconocido;

        if (mensaje.Contains("hola") || mensaje.Contains("buenas"))
            return ChatIntent.Saludo;

        if (mensaje.Contains("camioneta") || mensaje.Contains("camio"))
            return ChatIntent.BuscarCamioneta;

        if (mensaje.Contains("suv"))
            return ChatIntent.BuscarSUV;

        if (mensaje.Contains("auto"))
            return ChatIntent.BuscarAuto;

        if (mensaje.Contains("hilux") || mensaje.Contains("gol") || mensaje.Contains("fiesta"))
            return ChatIntent.BuscarModelo;

        if (mensaje.Contains("precio") || mensaje.Contains("cuanto"))
            return ChatIntent.Precio;

        if (mensaje.Contains("barato"))
            return ChatIntent.Barato;

        if (mensaje.Contains("caro") || mensaje.Contains("premium"))
            return ChatIntent.Caro;

        if (mensaje.Contains("financiacion") || mensaje.Contains("cuotas"))
            return ChatIntent.Financiamiento;

        if (mensaje.Contains("permuta"))
            return ChatIntent.Permuta;

        if (mensaje.Contains("donde") || mensaje.Contains("ubicacion"))
            return ChatIntent.Ubicacion;

        if (mensaje.Contains("horario"))
            return ChatIntent.Horario;

        if (mensaje.Contains("comprar") || mensaje.Contains("interesado"))
            return ChatIntent.InteresCompra;

        return ChatIntent.Desconocido;
    }
}
