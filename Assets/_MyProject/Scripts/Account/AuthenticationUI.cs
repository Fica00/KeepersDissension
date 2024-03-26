using UnityEngine;

public class AuthenticationUI : MonoBehaviour
{
   public static AuthenticationUI Instance;
   [SerializeField] private LoginUI login;
   [SerializeField] private RegisterUI register;
   [SerializeField] private ForgotPasswordUI forgotPassword;

   private void Awake()
   {
      Instance = this;
   }

   public void ShowLogin()
   {
      SwitchPanel(login);
   }

   public void ShowRegister()
   {
      SwitchPanel(register);
   }

   public void ShowForgotPassword()
   {
      SwitchPanel(forgotPassword);
   }

   private void SwitchPanel(IPanel _panel)
   {
      login.Close();
      register.Close();
      forgotPassword.Close();
      
      _panel.Setup();
   }
}