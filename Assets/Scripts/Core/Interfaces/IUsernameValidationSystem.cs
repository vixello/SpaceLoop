namespace Assets.Scripts.Core.Interfaces
{
    public interface IUsernameValidationSystem
    {
        UsernameValidationResult Validate(string username);
    }
}
