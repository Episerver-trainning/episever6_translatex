namespace EPiServer.Research.Translation4.Common
{
    public enum TranslationStatus
    {
        Created=1,
        ReadyForSend=2,
        Sending=3,
        Sent=4,
        ReadyForRecieve=5,
        Receiving=6,
        Received=7,
        ReadyForImport=8,
        Importing=9,
        Imported=10,
        NoSend=0
    }
}
