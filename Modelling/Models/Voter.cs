namespace Modelling.Models;
public sealed class Voter
{
    public Guid Id { get; private set; }

    public Guid RegistrationNumber { get; private set; }

    public void GenerateAndSetId()
    {
        Id = Guid.NewGuid();
    }

    public void SetRegistrationNumber(byte[] encryptedRegistrationNumber)
    {

    }
}
