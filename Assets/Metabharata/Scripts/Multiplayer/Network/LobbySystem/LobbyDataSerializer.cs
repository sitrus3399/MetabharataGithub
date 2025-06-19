using System.Text;
using Metabharata.Multiplayer.Network.LobbySystem;

public static class LobbyDataSerializer
{
    /// <summary>
    /// Serializes a LobbyDataModel to a UTF8 byte array.
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    public static byte[] Serialize(LobbyDataModel model)
    {
        var json = model.ToJson(); // Assumes you have a ToJson() extension or method
        return Encoding.UTF8.GetBytes(json);
    }

    /// <summary>
    /// Deserializes a UTF8 byte array to a LobbyDataModel.
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    public static LobbyDataModel Deserialize(byte[] data)
    {
        var json = Encoding.UTF8.GetString(data);
        return json.ToLobbyDataModel();
    }

    /// <summary>
    /// Deserializes a JSON string to a LobbyDataModel instance.
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public static LobbyDataModel Deserialize(string json)
    {
        return json.ToLobbyDataModel();
    }
}