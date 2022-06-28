using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Transactions;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SIMTech.APS.User.API.Controllers
{
    using SIMTech.APS.User.API.Repository;
    using SIMTech.APS.User.API.Models;
    using SIMTech.APS.User.API.PresentationModels;
    using SIMTech.APS.Utilities;
    using SIMTech.APS.User.API.Mappers;
    using SIMTech.APS.Models;
    using SIMTech.APS.PresentationModels;

    [Route("api/[controller]")]
    [ApiController]

    //[Authorize(Roles = "Administrator, Planner")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;


        public UserController(IUserRepository UserRepository)
        {
            _userRepository = UserRepository;
        }

        //GET: api/user
        [HttpGet]
        //public async Task<IEnumerable<User>> GetAllUsers() => await _userRepository.GetAll();

        public IEnumerable<UserPM> GetAllUsers()
        {
            var users = _userRepository.GetQuery(u => u.LoginId != "Dev").OrderBy(x => x.LoginId).ToList();

            var userPMs = UserMapper.ToPresentationModels(users).ToList();

            foreach (var userPM in userPMs)
            {
                userPM.Roles = "";
                var roles = FindUserRoleNames(userPM.Id);
                if (roles != null && roles.Count > 0)
                {
                    var role = roles.FirstOrDefault();
                    userPM.Roles = role.RoleName;
                    userPM.RoleID = role.Id;
                }

            }

            return userPMs.AsQueryable();
        }

        [HttpGet("Operator")]
        public IEnumerable<BasePM> GetOperators()
        {
            var users = _userRepository.GetQuery(u => u.LoginId != "Dev").OrderBy(x => x.LoginId).Select (x=>new BasePM() { Id = x.Id, Name = x.LoginId, Description = x.FirstName }).ToList ();

            return users;
        }


        [HttpGet]
        [Route("{id}")]
        //public User GetUserById(int id) => _userRepository.GetById(id);

        public UserPM GetUserById(int id)
        {
            var user = _userRepository.GetById(id);
            return UserMapper.ToPresentationModel(user);
        }




        [HttpPost]
        public void AddUser([FromBody] UserPM userPM)
        {
            User existingUser = _userRepository.GetQuery(u => u.LoginId.Equals(userPM.Username)).FirstOrDefault();
            if (existingUser != null)
            {
                throw new Exception("Duplicated user name.");
            }
            else
            {

                User user = UserMapper.FromPresentationModel(userPM);
                user.PasswordSalt = CryptoServiceProvider.CreateRandomSalt();
                user.PasswordHash = CryptoServiceProvider.ComputeSaltedHash(userPM.Password, user.PasswordSalt);
                user.PasswordAnswerSalt = CryptoServiceProvider.CreateRandomSalt();
                if (!string.IsNullOrEmpty(userPM.PasswordAnswer)) user.PasswordAnswerHash = CryptoServiceProvider.ComputeSaltedHash(userPM.PasswordAnswer, user.PasswordAnswerSalt);
                user.CreatedOn = DateTime.Today;

                _userRepository.Insert(user);


                if (userPM.RoleID > 0)
                {
                    UpdateUserRole(user.Id, userPM.RoleID);
                }
            }
        }

        [HttpGet]
        [Route("Encrypt/{dbConnectionName}")]
        public string GetEncryptedConnectionString(string dbConnectionName)
        {
            var dbConnetionString = Environment.GetEnvironmentVariable(dbConnectionName);
            var wrapper = new DataProtection("#"+dbConnectionName+"#");
            var cipherText = wrapper.EncryptData(dbConnetionString);
            return cipherText;
        }

        [HttpGet]
        [Route("Decrypt/{dbConnectionName}")]
        public string GetDecryptedConnectionString(string dbConnectionName)
        {
            var wrapper = new DataProtection("#" + dbConnectionName + "#");

            var dbConnetionString = Environment.GetEnvironmentVariable(dbConnectionName);

            // DecryptData throws if the wrong password is used.
            try
            {
                var plainText = wrapper.DecryptData(dbConnetionString);
                return plainText;
            }
            catch (System.Security.Cryptography.CryptographicException ex)
            {
                Console.WriteLine(ex.Message);
            }

            return dbConnectionName;


        }


        [HttpPut]
        public void UpdateUser([FromBody] UserPM userPM)
        {
            //User user = UserMapper.FromPresentationModel(userPM);
            //_userRepository.Update(user);

            var user = UserMapper.FromPresentationModel(userPM);
            var existingUser = _userRepository.GetById(user.Id);

            existingUser.Comment = user.Comment;
            existingUser.Email = user.Email;
            existingUser.FailedPasswordAttemptCount = user.FailedPasswordAttemptCount;
            existingUser.FirstName = user.FirstName;
            existingUser.IsLockedOut = user.IsLockedOut;
            existingUser.LastLockoutDate = user.LastLockoutDate;
            existingUser.LastLoginDate = user.LastLoginDate;
            existingUser.LastName = user.LastName;
            existingUser.LastPasswordChangedDate = user.LastPasswordChangedDate;
            existingUser.Mobile = user.Mobile;
            existingUser.LoginId = user.LoginId;

            _userRepository.Update(existingUser);

            if (userPM.RoleID > 0)
            {
                UpdateUserRole(user.Id, userPM.RoleID);
            }
        }


        // DELETE api/<UserController>/5
        [HttpDelete("{id}")]
        public void DeleteUser(int id) => _userRepository.Delete(id);

        [HttpPost("ResetPW")]
        public IActionResult ResetPasswords([FromBody] string userName)
        {

            if (userName == "")
            {
                IEnumerable<User> users = _userRepository.GetQuery(u => u.LoginId != "Dev").ToList();

                foreach (User user in users)
                {
                    user.PasswordHash = CryptoServiceProvider.ComputeSaltedHash(user.LoginId, user.PasswordSalt);
                    user.LastPasswordChangedDate = null;
                    user.FailedPasswordAttemptCount = 0;
                    _userRepository.Update(user);
                }
            }
            else
            {
                User user = _userRepository.GetQuery(u => u.LoginId.Equals(userName)).FirstOrDefault();
                user.PasswordHash = CryptoServiceProvider.ComputeSaltedHash(user.LoginId, user.PasswordSalt);
                user.LastPasswordChangedDate = null;
                user.FailedPasswordAttemptCount = 0;
                user.ModifiedOn = DateTime.Now;
                _userRepository.Update(user);

            }

            return new OkObjectResult(null);
        }


        [HttpPost("Login")]
        //[ValidateAntiForgeryToken]
        public IActionResult Login([FromBody] LoginPM loginPM)
        {
            string userName = loginPM.UserName;
            string password = loginPM.Password;

            bool isPasswordPolicyEnabled = false;
            bool isdev = false;
            var optionpwd = GetOptionByName("EnablePasswordPolicy");


            User existingUser = _userRepository.GetUserByName(userName);
            if (optionpwd != null && optionpwd.DefaultSetting != null && optionpwd.DefaultSetting.ToUpper() == "T")
            {
                isPasswordPolicyEnabled = true;
                if (existingUser != null)
                {
                    var roleName = FindUserRoleName(existingUser.Id);
                    if (roleName.Trim() == "DEVELOPER")
                    {
                        isdev = true;
                    }
                    //if (existingUser.UserRoles != null && existingUser.UserRoles.Count > 0)
                    //{
                    //    var objuserrole = existingUser.UserRoles.FirstOrDefault();
                    //    if (objuserrole != null && objuserrole.Role != null && objuserrole.Role.RoleName.Trim() == "DEVELOPER")
                    //    {
                    //        isdev = true;
                    //    }
                    //}

                    if (!isdev && ((existingUser.FailedPasswordAttemptCount ?? 0) >= 4))
                    {
                        //Your account is locked. Please contact Administrator to reset the Password!
                        //throw new DomainException("Invalid username or password.", -5);
                        throw new Exception("Invalid username or password");
                    }
                }

            }

            if (!ValidateUser(userName, password))
            {
                if (isPasswordPolicyEnabled && !isdev && existingUser != null && existingUser.Id > 0)
                {
                    int exitcount = (existingUser.FailedPasswordAttemptCount ?? 0);
                    existingUser.FailedPasswordAttemptCount = exitcount + 1;
                    _userRepository.Update(existingUser);
                    _userRepository.Save();
                    //Password is Incorrect(Account will lock after 5 failed attempts).
                    //throw new Exception("Invalid username or password.");               
                    return StatusCode(Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized, "Invalid username or password.");

                }
                else
                {
                    //throw new Exception("Invalid username or password.");
                    return StatusCode(Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized, "Invalid username or password.");
                }

            }

            if (!CheckConcurrentUser(userName))
            {
                throw new Exception("Maximum number of concurrent user is exceeded. Please wait for other users to quit or buy more licences!");
            }

            // Ravi Dec-18-2019 Password policy


            if (isPasswordPolicyEnabled)
            {
                #region "PASSWORD POLICY"



                if (existingUser != null)
                {
                    int validMonths = 3;
                    DateTime dtactive = DateTime.Now.AddDays(-92);
                    //Option optionmonths = _settingRepository.GetQuery<Option>(op => op.OptionName.Trim().Equals("LoginPasswordValid_Months")).SingleOrDefault();

                    var optionmonths = GetOptionByName("EnablePasswordPolicy");
                    if (optionmonths != null && optionmonths.DefaultSetting != null)
                    {
                        Int32.TryParse(optionmonths.DefaultSetting.ToString(), out validMonths);
                        if (validMonths <= 0)
                        {
                            validMonths = 3;
                        }
                        dtactive = DateTime.Now.AddDays(-(validMonths * 30));
                    }

                    DateTime dt3months = DateTime.Now.AddDays(-92);
                    DateTime dtUserlogin = (existingUser.LastLoginDate ?? existingUser.CreatedOn);
                    if (!isdev && (dtUserlogin < dtactive))
                    {
                        //Your account is inactive from past few months. Please contact Admin to reset the Password!
                        throw new Exception("Your account is inactive from past few months. Please contact Admin to reset the Password!");
                    }


                    // First time login users need to change the password
                    if (existingUser.LastPasswordChangedDate == null)
                    {
                        throw new Exception("Firstime Login users are mandatory to change the Password!");
                    }

                    if (existingUser.LastPasswordChangedDate != null && existingUser.LastPasswordChangedDate.HasValue)
                    {
                        dtUserlogin = (existingUser.LastPasswordChangedDate ?? DateTime.Now);
                        if (dtUserlogin < dt3months)
                        {
                            throw new Exception("Your password is expired! Please change the Password!");
                        }
                    }

                }
                else
                {
                    throw new Exception("Invalid username or password.");
                }

                #endregion
            }


            //Not supported by .net core, to be fixed later
            //FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1, userName, DateTime.Now, DateTime.Now.AddDays(7), isPersistent, "A", FormsAuthentication.FormsCookiePath);
            //string encryptedTicket = FormsAuthentication.Encrypt(ticket);

            //HttpCookie httpCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
            //if (ticket.IsPersistent) httpCookie.Expires = ticket.Expiration;

            //HttpContextBase httpContext = (HttpContextBase)ServiceContext.GetService(typeof(HttpContextBase));
            //httpContext.Response.Cookies.Add(httpCookie);

            ResetLoginCount(userName, true);

            var userPM = GetUserByName(userName);

            return new OkObjectResult(userPM);


        }

        [HttpPost("Logout")]
        public IActionResult Logout()
        {
            //FormsAuthentication.SignOut();
            var anonymousUser = new LoginUserPM { Name = String.Empty, Roles = new List<string>() };
            var user1 = new User() { Id = 100, LoginId = "Test" };
            return new OkObjectResult(user1);
        }


        private bool ValidateUser(string username, string password)
        {
            LoginUserPM existingUser = GetUserByName(username);
            if (existingUser == null) return false;

            string passwordHash = CryptoServiceProvider.ComputeSaltedHash(password, existingUser.PasswordSalt);
            return string.Equals(passwordHash, existingUser.PasswordHash, StringComparison.Ordinal);
        }




        private bool CheckConcurrentUser(string userName)
        {
            int maxCount = 1;

            //Option option = _exceptionManager.Process(() => _settingRepository.GetQuery<Option>(op => op.OptionName.Trim().Equals("SMCU")).SingleOrDefault(), "ExceptionShielding");

            var option = GetOptionByName("SMCU");
            //for (int i=1; i<=50; i++)
            //    Debug.WriteLine (CryptoServiceProvider.ComputeSaltedHash(i.ToString() , "SMCU"));


            if (option != null)
            {
                var hash = CryptoServiceProvider.ComputeSaltedHash(option.DefaultSetting, "SMCU");

                if (string.Equals(hash, option.Description, StringComparison.Ordinal))
                    maxCount = int.Parse(option.DefaultSetting);
                else
                    maxCount = 0;

            }

            //int currentCount = _permissionRepository.GetQuery<User>(u => u.IsLockedOut != null && u.IsLockedOut == true && u.UserName != userName).Count();
            int currentCount = _userRepository.CountConcurrentUser(userName);

            return (currentCount < maxCount ? true : false);

        }

        private LoginUserPM GetUserByName(string username)
        {
            User existingUser = _userRepository.GetUserByName(username);
            if (existingUser == null) return null;

            LoginUserPM loginUser = new LoginUserPM
            {
                Name = existingUser.LoginId,
                UserName = existingUser.FirstName,
                PasswordHash = existingUser.PasswordHash,
                PasswordSalt = existingUser.PasswordSalt,
                LastPassChangeDate = existingUser.LastPasswordChangedDate,
                ChangePasswordError = string.Empty
            };


            loginUser.Roles = FindUserRoleNames(existingUser.Id).Select(x => x.RoleName).ToList();
            loginUser.Modules = FindUserModuleNames(existingUser.Id);

            //if (existingUser.UserRoles.Any())
            //{
            //    loginUser.Roles = existingUser.UserRoles.Select(role => role.Role.RoleName).ToList();
            //    loginUser.Modules = GetModulesByRole(existingUser.UserRoles.Select(ur => ur.Role));
            //}

            return loginUser;
        }

        private void ResetLoginCount(string userName, bool login)
        {
            User existingUser = _userRepository.GetUserByName(userName);

            if (login)
            {
                existingUser.IsLockedOut = true;
                existingUser.LastLoginDate = DateTime.Now;
                if ((existingUser.FailedPasswordAttemptCount ?? 0) >= 1 && (existingUser.FailedPasswordAttemptCount ?? 0) <= 4)
                {
                    existingUser.FailedPasswordAttemptCount = 0;
                }

            }
            else
            {
                existingUser.IsLockedOut = false;
                //existingUser.LastLoginDate = null;
            }
            _userRepository.Update(existingUser);
            _userRepository.Save();
        }

        #region Integration from other serice

        //get from permission service
        private string FindUserRoleName(int userId)
        {
            return "Admin";
        }

        private List<Role> FindUserRoleNames(int userId)
        {
            using (var client = new System.Net.Http.HttpClient())
            {
                //client.BaseAddress = new Uri("http://host.docker.internal:5107/api/role/user/");
                client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("RPS_PERMISSION_URL") + "/api/role/user/");


                //HTTP GET
                try
                {
                    var responseTask = client.GetAsync(userId.ToString());
                    responseTask.Wait();

                    var result = responseTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsStringAsync();
                        readTask.Wait();

                        var alldata = readTask.Result;
                        var roles = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Role>>(alldata);
                        return roles;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("{0} Exception caught.", e);
                }

            }
            return new List<Role>();
            //return new List<string>() { "Administrator" };
        }

        private List<string> FindUserModuleNames(int userId)
        {
            using (var client = new System.Net.Http.HttpClient())
            {
                //client.BaseAddress = new Uri("http://host.docker.internal:5107/api/role/module/");
                client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("RPS_PERMISSION_URL") + "/api/role/module/");
                //HTTP GET
                try
                {
                    var responseTask = client.GetAsync(userId.ToString());
                    responseTask.Wait();

                    var result = responseTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsStringAsync();
                        readTask.Wait();

                        var alldata = readTask.Result;
                        var roles = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(alldata);
                        return roles;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("{0} Exception caught.", e);
                }


            }
            return null;

        }

        private void UpdateUserRole(int userId, int roleId)
        {
            using (var client = new System.Net.Http.HttpClient())
            {
                //client.BaseAddress = new Uri("http://host.docker.internal:5107/api/role/userrole/");
                client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("RPS_PERMISSION_URL") + "/api/role/userrole/");
                //HTTP Post

                //Create my object
                var userRole = new UserRole() { UserId = userId, RoleId = roleId };

                try
                {
                    string data = Newtonsoft.Json.JsonConvert.SerializeObject(userRole);

                    System.Net.Http.HttpContent content = new System.Net.Http.StringContent(data, System.Text.UTF8Encoding.UTF8, "application/json");

                    var responseTask = client.PostAsync(client.BaseAddress, content);

                    responseTask.Wait();

                    var result = responseTask.Result;
                    if (!result.IsSuccessStatusCode)
                    {
                        throw new Exception("Failed in inserting role");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("{0} Exception caught.", e);
                }



            }
        }


        //get from setting service
        private Option GetOptionByName(string optionName)
        {
            var option = new Option() { OptionName = "EnablePasswordPolicy" };

            switch (optionName)
            {

                case "EnablePasswordPolicy":
                    option.DefaultSetting = "F";
                    break;
                case "LoginPasswordValid_Months":
                    option.DefaultSetting = "3";
                    break;
                case "SMCU":
                    option.DefaultSetting = "50";
                    option.Description = "mZKzunq90fKLp9Stq6z7tNhhSGE9zJpoj7KUQuX2njWgQl5+0y9+0v6mxlrQIZMirxGBFmtRRrh2+whAve2cUw==";
                    break;
                default:
                    return new Option();

            }

            return option;


        }
        #endregion
    }

}
