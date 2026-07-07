using Aksl.Dialogs.Services;
using Aksl.Infrastructure;
using Aksl.Infrastructure.Events;
using Aksl.Modules.Account.ViewModels;
using Aksl.Modules.Account.Views;
using Aksl.Toolkit.Controls;
using Prism;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace Aksl.Modules.Shell
{
    public static class DialogViewServiceExtensions
    {
        public static async Task ShowLoginDialogAsync(this IDialogViewService dialogViewService)
        {
            var parameters = new DialogParameters {{"Title",  "请 登 陆" },{ "WindowCloseButtonVisibility", "Visible"}, { "Width",650d }, { "Height", 350d },
                                                  { "OkText",  "登 陆" },{"OkIconKind",PackIconKind.AccountAdd},{"OkToolTip","登陆"  },{"CancelText","Cancel"  },
                                                  {"UserNameWater","用户名"  },{"PasswordWater","密码"  }};

            #region Method
            //var loginPopupView = Container.Resolve<LoginPopupView>();
            //var loginPopupViewModel = loginPopupView.DataContext as LoginPopupViewModel;

            //loginPopupViewModel.RequestClose += (result) =>
            //{
            //    if (result.Result == ButtonResult.Cancel)
            //    {
            //        Shutdown();
            //    }
            //    if (result.Parameters.TryGetValue("LoginPopupViewModel", out LoginPopupViewModel loginPopupViewModel))
            //    {
            //        if (!loginPopupViewModel.IsSuccessful)
            //        {
            //            Shutdown();
            //        }

            //        if (loginPopupViewModel.IsSuccessful)
            //        {
            //            string userName = loginPopupViewModel.UserName;
            //        }
            //    }
            //};

            //var dialogResult =await dialogViewService.ShowDialogAsync(dialogContent: loginPopupView);
            #endregion

            #region Method
            try
            {
                // 使用可取消的 token（例如 60 秒超时），防止对话框未触发回调导致等待无限期挂起
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));

                var dialogResult = await dialogViewService.ShowDialogAsync<LoginPopupView>(parameters: parameters, cancellationToken: CancellationToken.None);

                if (dialogResult is null)
                {
                    // 如果返回 null，视为取消或未正常完成
                    App.Current.Shutdown();
                }

                if (dialogResult.Result == ButtonResult.Cancel)
                {
                   //dialogViewService.AlertAsync(message: "登录取消", title: "登 录").Await();

                   //System.Windows.Application.Current.Shutdown();
                }
                else if (dialogResult.Result == ButtonResult.OK)
                {
                    if (dialogResult.Parameters.TryGetValue("LoginPopupViewModel", out LoginPopupViewModel loginPopupViewModel))
                    {
                        if (!loginPopupViewModel.IsSuccessful)
                        {
                            App.Current.Shutdown();
                        }
                        if (loginPopupViewModel.IsSuccessful)
                        {
                            PrismUnityExtensions.GetEventAggregator()
                                                .GetEvent<OnSignInedEvent>()
                                                .Publish(new OnSignInedEvent { UserName = loginPopupViewModel.UserName, IsSuccessful = true });
                            //dialogViewService.AlertAsync(message: "登录成功", title: "登 录").Await();
                        }
                    }
                }
            }
            catch (OperationCanceledException ocex)
            {
                await dialogViewService.AlertAsync(message: $"{ocex.Message}", title: "登录");
                App.Current.Shutdown();
            }
            catch (Exception ex)
            {
                string msg = !string.IsNullOrEmpty(ex.InnerException?.Message) ? ex.InnerException.Message : ex.Message;
                await dialogViewService.AlertAsync(message: msg, title: "登录");

                System.Windows.Application.Current.Shutdown();
            }
            #endregion
        }
    }
}
