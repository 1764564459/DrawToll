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
                this.Lab.Content= dialog.FileName;
                var deskFile = dialog.FileName;
                var path = System.IO.Path.GetDirectoryName(deskFile);
                SetFileRole(path);
                //复制JQuery
                var result=  CopyJquery(path);
                
                //创建Js
                result= CreateDrawJs(path,deskFile);
                if (result != null)//创建Js、复制JQuery失败
                {
                    MessageBox.Show(result);
                    //提示信息
                    return;
                }
                if (!deskFile.Any())
                {
                    MessageBox.Show("未选择任何文件！！！");
                    //提示信息
                    return;
                }
               
            }
        }

        /// <summary>
        /// 创建Js
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string CreateDrawJs(string path,string filePath)
        {
            try
            {
                string html = "document.onmousemove=function(e,t)"
                           + "{"
                               + "var _bRst=false;"
                               + "var _div= document.getElementsByTagName(\"div\");"
                               + "for(var i=0;i<_div.length;i++)"
                               + "{"
                               + "    if($(_div[i]).text()==\"Button\")"
                               + "    {"
                                   + "  _div[i].onclick=function(e)" +
                                       "{"
                                       + "	  if(_bRst)"
                                       + "	   return true;"
                                       + "	   debugger;"
                                       + "	   console.log($(e));"
                                       + "	_bRst=true;"
                                   + "}"
                               + "   }"
                             + "}"
                           + "}";
                var drawJs = File.Create(@$"{path}\Draw.js");
                SetFileAccess(@$"{path}\Draw.js");
                byte[] _byte = Encoding.UTF8.GetBytes(html);
                drawJs.Write(_byte, 0, html.Length);

                SetFileAccess(filePath);
                FileStream file = new FileStream(filePath,FileMode.Append );// File.OpenWrite(filePath);
                _byte = Encoding.UTF8.GetBytes("<script type=\"text/javascript\" src=\"./jquery.js \"></script> \r\n<script type=\"text/javascript\" src=\"./Draw.js \"></script>");
                file.Write(_byte, 0, _byte.Length);

                drawJs.Close();
                drawJs.Dispose();
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
        /// 复制Jquery
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        private string CopyJquery(string address)
        {
            try
            {
                var path = $@"{(System.IO.Path.GetDirectoryName(typeof(MainWindow).Assembly.Location))}\wwwroot\jquery.js";
                if (File.Exists(path))
                {
                    File.Copy(path, $"{address}/jquery.js");
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
    }
}
