/// <summary>
/// Traduce los IDs numéricos de "cards" en el db.json (1..52) a los códigos
/// de 2 caracteres que espera la Deck of Cards API (por ejemplo "AS" = As
/// de Picas, "0H" = 10 de Corazones, "KC" = Rey de Tréboles).
///
/// Orden de la baraja estándar que se usa para mapear:
///   IDs 1-13  -> Picas    (S) : A,2,3,4,5,6,7,8,9,10,J,Q,K
///   IDs 14-26 -> Corazones(H) : A,2,3,4,5,6,7,8,9,10,J,Q,K
///   IDs 27-39 -> Diamantes(D) : A,2,3,4,5,6,7,8,9,10,J,Q,K
///   IDs 40-52 -> Tréboles (C) : A,2,3,4,5,6,7,8,9,10,J,Q,K
///
/// Nota: en el código de la Deck of Cards API el "10" se representa como
/// "0" (por ejemplo "0S" = 10 de Picas), porque los códigos son de 2
/// caracteres exactos.
/// </summary>
public static class CardCodeMapper
{
    private static readonly string[] RankCodes =
    {
        "A", "2", "3", "4", "5", "6", "7", "8", "9", "0", "J", "Q", "K"
    };

    private static readonly char[] SuitCodes = { 'S', 'H', 'D', 'C' };

    /// <summary>Convierte un ID (1..52, se envuelve si sale de rango) en un código de 2 caracteres, ej: 1 -> "AS".</summary>
    public static string IdToCode(int id)
    {
        int zeroBased = Wrap(id - 1, 52);
        int suitIndex = zeroBased / 13;
        int rankIndex = zeroBased % 13;
        return RankCodes[rankIndex] + SuitCodes[suitIndex];
    }

    private static int Wrap(int value, int mod)
    {
        int result = value % mod;
        return result < 0 ? result + mod : result;
    }
}
