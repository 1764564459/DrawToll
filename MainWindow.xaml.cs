using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
namespace DrawTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _selectFile = string.Empty;
        public  MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 选择文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Open_File(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog() { Multiselect=false };
           dialog.Filters.Add(new CommonFileDialogFilter("HTML Files", "*.html"));
           //dialog.Filters.Add(new CommonFileDialogFilter("AVI Files", "*.avi"));
           //dialog.Filters.Add(new CommonFileDialogFilter("MP3 Files", "*.mp3"));
           // dialog.Filters.Add(new CommonFileDialogFilter("MKV Files", "*.mkv"));

            var Result= dialog.ShowDialog();
            if(Result== CommonFileDialogResult.Ok)
            {
               
                var deskFile = dialog.FileName;
               
                if (!deskFile.Any())
                {
                    MessageBox.Show("未选择任何文件！！！");
                    //提示信息
                    return;
                }
                _selectFile = deskFile;
            }
        }

        /// <summary>
        /// 创建Js
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string CreateDrawJs(string path,string filePath,string setName="#Button")
        {
            try
            {
                //创建Draw.js
                if (File.Exists(@$"{path}\Draw.js"))
                    File.Delete(@$"{path}\Draw.js");
                var drawJs = File.Create(@$"{path}\Draw.js");

                //设置文件访问权限
                SetFileAccess(@$"{path}\Draw.js");
                drawJs.Close();
                drawJs.Dispose();
                //写入js
                WriteDrawJs(path,setName);

                //绑定需要绑的事件 
                //BindClickEvent(path, setName);

                //设置html访问权限
                SetFileAccess(filePath);
                FileStream file = new FileStream(filePath,FileMode.Append );// File.OpenWrite(filePath);
                byte[] _byte = Encoding.UTF8.GetBytes("\r\n <script type=\"text/javascript\" src=\"./jquery.js \"></script> \r\n<script type=\"text/javascript\" src=\"./Draw.js \"></script> \r\n<script type=\"text/javascript\" src=\"./socket.js \"></script> \r\n");
                file.Write(_byte, 0, _byte.Length);

               
                file.Close();
                file.Dispose();
                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// 绑定事件
        /// </summary>
        /// <param name="element"></param>
        private string BindClickEvent(string element)
        {
            element = element.Replace("，", ",").Replace("：",":");
            string[] _arr = element.Split(',');
            string _events = string.Empty;
            foreach (var item in _arr)
            {
                string[] _send = item.Split(":");
                string _msg = string.Empty;
                if (_send.Length > 2)
                    _msg = _send[1];
                _events += @"  var _"+item+ @"=$('body').contents().find('#"+item+ @"');
                               if ( _" + item + @"!= null &&  _" + item + @" != 'undefined') 
                               {
                                    for (var i = 0; i <  _" + item + @".length; i++) 
                                    {
                                        _" + item + @"[i].onclick = function (e) 
                                        {
                                            alert('123')
                                        }
                                    }
                               }  ";
            }
            return _events;
        }

        /// <summary>
        /// 写入Js
        /// </summary>
        /// <param name="filePath"></param>
        private void WriteDrawJs(string filePath,string element)
        {
            var _event = BindClickEvent(element);
            string html= @"
                            document.onmousemove=function(e,f)
                            {
                                "+_event+ @"
                                var _idArr = [];
                                $('body').contents().find('text,div,span').each(function () {
                                    var isBindedId = $(this).attr('isBindedId');
                                    if (!!isBindedId && isBindedId == 'True')
                                    {
                                        _idArr.push($(this).attr('id'));
                                        return true;
                                    }
                                     var curObjId = $(this).html().replace(/\s+/g, '');
                                     //验证ID是否正确
                                    if (curObjId == undefined || curObjId == '')
                                        return true;
                                    var tempArr = curObjId.split('_');
                                    //if (tempArr.length < 2) return true;
                                    $(this).attr('id', curObjId);
                                    $(this).attr('isBindedId', 'True');
                                    _idArr.push(curObjId);
                                });
                            var _data = { water: '122', gas: '123', heat: '45621', fast: 125 };
                            BindData(_data);
                            function BindData(data)
                            {
                                $.each(data, function (index, item) {
                                    var current = $('body').contents().find('#' + index);
                                    if (current != null && current != 'undefined') {
                                        for (var i = 0; i < current.length; i++)
                                        {
                                          current[i].textContent=item;
                                        }
                                    }
                                });
                            }
                        }
                        
                     var _src='"+this.webscoket.Text+@"';
                    ";
            FileStream file = new FileStream(@$"{filePath}\Draw.js", FileMode.Append);
            byte[] _byte = Encoding.UTF8.GetBytes(html);
            file.Write(_byte, 0, _byte.Length);
            file.Close();
            file.Dispose();
        }


        /// <summary>
        /// 复制Jquery
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        private string CopyJquery(string address)
        {
            try
            {
                string path = $@"{(System.IO.Path.GetDirectoryName(typeof(MainWindow).Assembly.Location))}\wwwroot\";
                string[] _files = new[] 
                { 
                    "jquery.js",
                    "socket.js",
                };
                foreach (var item in _files)
                {
                    if (File.Exists($"{path}{item}"))
                    {
                        if (File.Exists($"{address}/{item}"))
                            File.Delete($"{address}/{item}");

                        File.Copy($"{path}{item}", $"{address}/{item}");
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// 设置文件夹权限，处理为Everyone所有权限
        /// </summary>
        /// <param name="foldPath">文件夹路径</param>
        private void SetFileRole(string foldPath)
        {
            //DirectorySecurity fsec = new DirectorySecurity();
            //fsec.SetAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.FullControl,
            //InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
            //fsec.AddAccessRule(new FileSystemAccessRule
            //       ("Everyone", FileSystemRights.FullControl,
            //AccessControlType.Allow));
            //FileInfo fi = new FileInfo(foldPath);
            //FileSecurity fileSecurity = fi.GetAccessControl();
            //fileSecurity.AddAccessRule
            //    (new FileSystemAccessRule
            //        ("Everyone", FileSystemRights.FullControl,
            //        AccessControlType.Allow));
            //fi.SetAccessControl(fileSecurity);

            // 获取文件夹信息
            try
            {
                DirectoryInfo dir = new DirectoryInfo(foldPath);
                //获得该文件夹的所有访问权限
                System.Security.AccessControl.DirectorySecurity dirSecurity = dir.GetAccessControl(AccessControlSections.All);
                //设定文件ACL继承
                InheritanceFlags inherits = InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit;
                //添加ereryone用户组的访问权限规则 完全控制权限
                FileSystemAccessRule everyoneFileSystemAccessRule = new FileSystemAccessRule("Everyone", FileSystemRights.FullControl, inherits, PropagationFlags.None, AccessControlType.Allow);
                //添加Users用户组的访问权限规则 完全控制权限
                FileSystemAccessRule usersFileSystemAccessRule = new FileSystemAccessRule("Users", FileSystemRights.FullControl, inherits, PropagationFlags.None, AccessControlType.Allow);
                bool isModified = false;
                dirSecurity.ModifyAccessRule(AccessControlModification.Add, everyoneFileSystemAccessRule, out isModified);
                dirSecurity.ModifyAccessRule(AccessControlModification.Add, usersFileSystemAccessRule, out isModified);
                //设置访问权限
                dir.SetAccessControl(dirSecurity);
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// 设置文件访问权限
        /// </summary>
        /// <param name="path"></param>
        private void SetFileAccess(string path)
        {
            FileInfo fi = new FileInfo(path);
            FileSecurity fileSecurity = fi.GetAccessControl();
            fileSecurity.AddAccessRule
                (new FileSystemAccessRule
                    ("Everyone", FileSystemRights.FullControl,
                    AccessControlType.Allow));
            fi.SetAccessControl(fileSecurity);
        }

        /// <summary>
        /// 写入配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Write_Setting(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrWhiteSpace(this.webscoket.Text))
            {
                MessageBox.Show("请输入websocket地址。");
                return;
            }    
            var path = System.IO.Path.GetDirectoryName(_selectFile);
            SetFileRole(path);
            //复制JQuery
            var result = CopyJquery(path);

            var setName = this.setName.Text;
            if (string.IsNullOrWhiteSpace(setName)) setName = "#Button";
            //创建Js
            result = CreateDrawJs(path, _selectFile,setName);
            if (result != null)//创建Js、复制JQuery失败
            {
                MessageBox.Show(result);
                //提示信息
                return;
            }
            MessageBox.Show("配置成功！！！");
        }
    }
}
