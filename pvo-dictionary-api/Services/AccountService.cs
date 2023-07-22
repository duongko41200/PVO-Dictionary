using AutoMapper;
using AutoMapper.Configuration;
using pvo_dictionary_api.Common;
using pvo_dictionary_api.Models;
using pvo_dictionary_api.Repositories;
using pvo_dictionary_api.Request;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Net.Mail;
using pvo_dictionary_api.Dto;
using System.Collections;

namespace pvo_dictionary_api.Services
{
    public class AccountService
    {
        private readonly UserRepository _userRepository;
        private readonly ApiOption _apiOption;
        private readonly IMapper _mapper;
        private readonly DictionaryRepository _dictionaryRepository;
        private readonly AppDbContext _databaseContext;
        public AccountService(ApiOption apiOption, AppDbContext databaseContext, IMapper mapper)
        {
            _userRepository = new UserRepository(apiOption, databaseContext, mapper);
            _apiOption = apiOption;
            _mapper = mapper;
            _dictionaryRepository = new DictionaryRepository(apiOption, databaseContext, mapper);
            _databaseContext = databaseContext;
        }

        /// <summary>
        /// User login
        /// </summary>
        /// <param name="userLoginRequest"></param>
        /// <returns></returns>
        public object UserLogin(UserLoginRequest userLoginRequest)
        {
            try
            {
                var user = _userRepository.UserLogin(userLoginRequest);
                if (user == null)
                {
                    throw new ValidateError(1000, "Incorrect email or password");
                }
                if (user.status == 0)
                {
                    throw new ValidateError(1004, "Unactivated account");
                }
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_apiOption.Secret));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
                var claimList = new[]
                {
            new Claim(ClaimTypes.Role, "User"),
            new Claim(ClaimTypes.UserData, user.user_name),
            new Claim(ClaimTypes.Sid, user.user_id.ToString()),
        };
                var token = new JwtSecurityToken(
                    issuer: _apiOption.ValidIssuer,
                    audience: _apiOption.ValidAudience,
                    expires: DateTime.Now.AddDays(1),
                    claims: claimList,
                    signingCredentials: credentials
                );
                var tokenByString = new JwtSecurityTokenHandler().WriteToken(token);

                // Retrieve all dictionaries associated with the user
                var dictionaries = _databaseContext.dictionaries
                    .Where(d => d.user_id == user.user_id)
                    .Select(d => new
                    {
                        DictionaryId = d.dictionary_id,
                        DictionaryName = d.dictionary_name
                    })
                    .ToList();

                return new
                {
                    token = tokenByString,
                    UserId = user.user_id,
                    UserName = user.user_name,
                    DisplayName = user.display_name,
                    Avatar = user.avatar,
                    Dictionaries = dictionaries // Return the list of dictionaries
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// account/register
        /// </summary>
        /// <param name="userRegisterRequest"></param>
        /// <returns></returns>
        public object UserRegister(UserRegisterRequest userRegisterRequest)
        {
            try
            {
                var user = _userRepository.GetByCondition(row => row.user_name == userRegisterRequest.username).FirstOrDefault();
                if (user != null)
                {
                    throw new ValidateError(1001, "Email has been used");
                }
                if (string.IsNullOrEmpty(userRegisterRequest.username))
                {
                    throw new ValidateError(9000, "Email is required");
                }

                // Check if password is empty
                if (string.IsNullOrEmpty(userRegisterRequest.password))
                {
                    throw new ValidateError(9000, "Password is required");
                }

                // Check email format
                if (!IsValidEmail(userRegisterRequest.username))
                {
                    throw new ValidateError(9000, "Incorrect email format");
                }

                // Check password format
                if (!IsValidPassword(userRegisterRequest.password))
                {
                    throw new ValidateError(9000, "Incorrect password format");
                }
                var newUser = new User()
                {
                    user_name = userRegisterRequest.username,
                    password = Untill.CreateMD5(userRegisterRequest.password),
                    email = userRegisterRequest.username,
                    status = 0,
                };
                _userRepository.Create(newUser);
                _userRepository.SaveChange();
                return newUser;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Send Activate Email
        /// </summary>
        /// <param name="sendActivateEmailRequest"></param>
        /// <returns></returns>
        public bool SendActivateEmail(SendActivateEmailRequest sendActivateEmailRequest)
        {
            try
            {
                var user = _userRepository.GetByCondition(row => row.user_name == sendActivateEmailRequest.username && row.password == Untill.CreateMD5(sendActivateEmailRequest.password)).FirstOrDefault();
                if (user == null)
                {
                    throw new ValidateError(1002, "Invalid account");
                }
                var tb =
                    "Bạn đã đăng ký tài khoản thành công" +
                    "<a href=\""+ _apiOption.BaseUrl + "/api/account/activate_account?username=" + sendActivateEmailRequest.username+"\">Bấm vào đây để active tài khoản</a>";

                MailMessage mail = new MailMessage("truongngocquyen11082000@gmail.com", sendActivateEmailRequest.username, "PVO Dictionary send email active account", tb);
                mail.IsBodyHtml = true;
                //gửi tin nhắn
                SmtpClient client = new SmtpClient("smtp.gmail.com");
                client.Host = "smtp.gmail.com";
                client.UseDefaultCredentials = false;
                client.Port = 587; 
                client.Credentials = new System.Net.NetworkCredential("truongngocquyen11082000@gmail.com", "vgjkystqsttwpyjc");
                client.EnableSsl = true; 
                client.Send(mail);
                
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Activate Account
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public bool ActivateAccount(string username)
        {
            try
            {
                var user = _userRepository.GetAll().Where(row => row.user_name == username).FirstOrDefault();
                if (user == null)
                {
                    return false;
                }
                user.status = 1;
                _userRepository.UpdateByEntity(user);
                _userRepository.SaveChange();
                
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// forgot password
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public string ForgotPassword(string email)
        {
            try
            {
                var user = _userRepository.GetAll().Where(row => row.user_name == email).FirstOrDefault();
                if (user == null)
                {
                    throw new ValidateError(1002, "Invalid account");
                }

                var listNumber = new List<string> { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
                var otp = "";
                Random rand = new Random();
                for (int i = 0; i < 4; i++)
                {
                    var randomNumber = rand.Next(0, 9);
                    otp += listNumber[randomNumber];
                }
                user.otp = otp;
                _userRepository.UpdateByEntity(user);
                _userRepository.SaveChange();

                var tb = "Mã OTP Code của bạn là: " + user.otp;

                MailMessage mail = new MailMessage("truongngocquyen11082000@gmail.com", user.user_name, "PVO Dictionary send OTP to reset password", tb);
                mail.IsBodyHtml = true;
                SmtpClient client = new SmtpClient("smtp.gmail.com");
                client.Host = "smtp.gmail.com";
                client.UseDefaultCredentials = false;
                client.Port = 587;
                client.Credentials = new System.Net.NetworkCredential("truongngocquyen11082000@gmail.com", "vgjkystqsttwpyjc");
                client.EnableSsl = true;
                client.Send(mail);

                return otp;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// reset password
        /// </summary>
        /// <param name="resetPasswordRequest"></param>
        /// <returns></returns>
        public bool ResetPassword(ResetPasswordRequest resetPasswordRequest)
        {
            try
            {
                var user = _userRepository.GetAll().Where(row => row.user_name == resetPasswordRequest.Email).FirstOrDefault();
         
                if (user == null)
                {
                    throw new ValidateError(9000, "Email doesn’t exist");
                }

                if(user.otp != resetPasswordRequest.Otp)
                {
                    throw new ValidateError(9000, "OTP invalid!");
                }

                user.password = Untill.CreateMD5(resetPasswordRequest.NewPassword);
                _userRepository.UpdateByEntity(user);
                _userRepository.SaveChange();

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var address = new System.Net.Mail.MailAddress(email);
                return address.Address == email;
            }
            catch
            {
                return false;
            }
        }

       

        private bool IsValidPassword(string password)
        {
            // Password should have at least 1 lowercase letter, 1 uppercase letter, 1 digit, 1 special character, and be 8 to 16 characters long
            var regex = new System.Text.RegularExpressions.Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*\W).{8,16}$");
            return regex.IsMatch(password);
        }


    }
}
