namespace Services.EmailTemplates;

public static class GastrofestEmailTemplates
{
    // --- 1. Ответ пользователю с формы контактов ---
    public static string BuildContactUserEmail(string userName, string userMessage)
    {
        var safeName = string.IsNullOrWhiteSpace(userName) ? "гость" : userName.Trim();

        return $@"
<!DOCTYPE html>
<html lang=""ru"">
<head>
  <meta charset=""utf-8"" />
  <title>Ваше обращение на сайт GastroFest получено</title>
</head>
<body style=""margin:0;padding:0;background:#fff4ea;font-family:system-ui,-apple-system,BlinkMacSystemFont,'Segoe UI',sans-serif;"">
  <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background:#fff4ea;padding:24px 0;"">
    <tr>
      <td align=""center"">
        <table width=""600"" cellpadding=""0"" cellspacing=""0"" border=""0"" 
               style=""background:#ffffff;border-radius:16px;border:1px solid #f5d8c2;
                      box-shadow:0 12px 35px rgba(0,0,0,0.06);padding:24px 28px;"">
          <tr>
            <td style=""font-size:22px;font-weight:700;color:#222;"">
              GastroFest<span style=""color:#ff7a24;"">.</span>
            </td>
          </tr>

          <tr><td style=""height:12px""></td></tr>

          <tr>
            <td style=""font-size:16px;color:#222;"">
              Здравствуйте, {safeName}!
            </td>
          </tr>

          <tr><td style=""height:8px""></td></tr>

          <tr>
            <td style=""font-size:14px;color:#4a3c30;line-height:1.6;"">
              Спасибо за ваше обращение на сайт <b>GastroFest</b>.<br/>
              Ваш запрос получен и будет обработан в ближайшее время.
            </td>
          </tr>

          <tr><td style=""height:18px""></td></tr>

          <tr>
            <td style=""font-size:13px;color:#777;margin:0 0 4px"">
              Текст вашего сообщения:
            </td>
          </tr>

          <tr>
            <td>
              <div style=""margin-top:4px;padding:12px 14px;border-radius:10px;
                          border:1px solid #f3e4d8;background:#fffaf5;
                          font-size:14px;color:#333;line-height:1.5;"">
                {System.Net.WebUtility.HtmlEncode(userMessage ?? string.Empty)}
              </div>
            </td>
          </tr>

          <tr><td style=""height:20px""></td></tr>

          <tr>
            <td style=""font-size:14px;color:#4a3c30;line-height:1.6;"">
              С уважением,<br/>
              команда <b>GastroFest</b>.
            </td>
          </tr>

          <tr><td style=""height:8px""></td></tr>

          <tr>
            <td style=""font-size:11px;color:#999;"">
              Это письмо отправлено автоматически, отвечать на него не нужно.
            </td>
          </tr>

        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
    }

    // --- 2. Письмо админу о новом обращении ---
    public static string BuildContactAdminEmail(string userName, string userEmail, string userMessage)
    {
        var safeName  = string.IsNullOrWhiteSpace(userName) ? "не указано" : userName.Trim();
        var safeEmail = string.IsNullOrWhiteSpace(userEmail) ? "не указано" : userEmail.Trim();

        return $@"
<!DOCTYPE html>
<html lang=""ru"">
<head>
  <meta charset=""utf-8"" />
  <title>Новое обращение через форму контактов GastroFest</title>
</head>
<body style=""margin:0;padding:0;background:#fff4ea;font-family:system-ui,-apple-system,BlinkMacSystemFont,'Segoe UI',sans-serif;"">
  <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background:#fff4ea;padding:24px 0;"">
    <tr>
      <td align=""center"">
        <table width=""600"" cellpadding=""0"" cellspacing=""0"" border=""0"" 
               style=""background:#ffffff;border-radius:16px;border:1px solid #f5d8c2;
                      box-shadow:0 12px 35px rgba(0,0,0,0.06);padding:24px 28px;"">
          <tr>
            <td style=""font-size:18px;font-weight:700;color:#222;"">
              Новое обращение с сайта GastroFest
            </td>
          </tr>

          <tr><td style=""height:10px""></td></tr>

          <tr>
            <td style=""font-size:14px;color:#4a3c30;line-height:1.6;"">
              С сайта поступило новое сообщение через форму контактов.
            </td>
          </tr>

          <tr><td style=""height:14px""></td></tr>

          <tr>
            <td style=""font-size:14px;color:#333;line-height:1.5;"">
              <b>Имя:</b> {System.Net.WebUtility.HtmlEncode(safeName)}<br/>
              <b>Email:</b> {System.Net.WebUtility.HtmlEncode(safeEmail)}
            </td>
          </tr>

          <tr><td style=""height:12px""></td></tr>

          <tr>
            <td style=""font-size:13px;color:#777;margin:0 0 4px"">
              Текст сообщения:
            </td>
          </tr>

          <tr>
            <td>
              <div style=""margin-top:4px;padding:12px 14px;border-radius:10px;
                          border:1px solid #f3e4d8;background:#fffaf5;
                          font-size:14px;color:#333;line-height:1.5;"">
                {System.Net.WebUtility.HtmlEncode(userMessage ?? string.Empty)}
              </div>
            </td>
          </tr>

          <tr><td style=""height:16px""></td></tr>

          <tr>
            <td style=""font-size:11px;color:#999;"">
              Письмо сгенерировано автоматически системой GastroFest.
            </td>
          </tr>

        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
    }

    // --- 3. Письмо для подтверждения регистрации ---
    public static string BuildRegistrationEmail(string userNameOrEmail, string confirmUrl)
    {
        var safeName = string.IsNullOrWhiteSpace(userNameOrEmail)
            ? "друг"
            : userNameOrEmail.Trim();

        var safeUrl = System.Net.WebUtility.HtmlEncode(confirmUrl ?? "#");

        return $@"
<!DOCTYPE html>
<html lang=""ru"">
<head>
  <meta charset=""utf-8"" />
  <title>Подтверждение регистрации на GastroFest</title>
</head>
<body style=""margin:0;padding:0;background:#fff4ea;font-family:system-ui,-apple-system,BlinkMacSystemFont,'Segoe UI',sans-serif;"">
  <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background:#fff4ea;padding:24px 0;"">
    <tr>
      <td align=""center"">
        <table width=""600"" cellpadding=""0"" cellspacing=""0"" border=""0"" 
               style=""background:#ffffff;border-radius:16px;border:1px solid #f5d8c2;
                      box-shadow:0 12px 35px rgba(0,0,0,0.06);padding:24px 28px;"">
          <tr>
            <td style=""font-size:22px;font-weight:700;color:#222;"">
              Добро пожаловать в GastroFest<span style=""color:#ff7a24;"">.</span>
            </td>
          </tr>

          <tr><td style=""height:10px""></td></tr>

          <tr>
            <td style=""font-size:16px;color:#222;"">
              Здравствуйте, {safeName}!
            </td>
          </tr>

          <tr><td style=""height:8px""></td></tr>

          <tr>
            <td style=""font-size:14px;color:#4a3c30;line-height:1.6;"">
              Вы зарегистрировались на сайте <b>GastroFest</b>.<br/>
              Пожалуйста, подтвердите свой email, чтобы завершить регистрацию.
            </td>
          </tr>

          <tr><td style=""height:18px""></td></tr>

          <tr>
            <td align=""left"">
              <a href=""{safeUrl}"" 
                 style=""display:inline-block;padding:10px 18px;border-radius:999px;
                        background:#ff7a24;color:#ffffff;text-decoration:none;
                        font-weight:600;font-size:14px;"">
                Подтвердить email
              </a>
            </td>
          </tr>

          <tr><td style=""height:14px""></td></tr>

          <tr>
            <td style=""font-size:12px;color:#777;line-height:1.5;"">
              Если кнопка не работает, скопируйте и вставьте ссылку в адресную строку браузера:<br/>
              <span style=""color:#555;word-break:break-all;"">{safeUrl}</span>
            </td>
          </tr>

          <tr><td style=""height:18px""></td></tr>

          <tr>
            <td style=""font-size:14px;color:#4a3c30;line-height:1.6;"">
              Если вы не регистрировались на нашем сайте, просто игнорируйте это письмо.
            </td>
          </tr>

          <tr><td style=""height:8px""></td></tr>

          <tr>
            <td style=""font-size:11px;color:#999;"">
              Письмо отправлено автоматически. Не отвечайте на него.
            </td>
          </tr>

        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
    }
}
