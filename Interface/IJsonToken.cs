namespace MyApiProject.Inerface
{
    public interface IJsonToken
    {
        string CreateToken(Guid id, string Name, string Email);
        Guid VerifyToken(string id);

     }
}