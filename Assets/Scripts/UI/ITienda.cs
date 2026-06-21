namespace DragonCeltas
{
    public interface ITienda
    {
        void Mostrar();
        void Ocultar();
        bool EstaAbierta { get; }
    }
}
