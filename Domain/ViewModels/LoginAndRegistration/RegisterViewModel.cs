namespace Domain.ViewModels.LoginAndRegistration
{
    public class RegisterViewModel
    {
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string ConfirmPassword { get; set; } = null!; // ← НОВОЕ СВОЙСТВО
    }
}
