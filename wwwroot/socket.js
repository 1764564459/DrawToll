var ws = null;
var _src = null;

//初始化websocket
function InitWs() {
    if ("WebSocket" in window) {
        // 打开一个 web socket
        ws = new WebSocket(_src);

        //打开
        ws.onopen = function () {
            // Web Socket 已连接上，使用 send() 方法发送数据

            alert("数据发送中...");
        };

        //收到消息
        ws.onmessage = function (evt) {
            var received_msg = evt.data;
            alert("数据已接收...");
        };

        //关闭
        ws.onclose = function () {
            // 关闭 websocket
            alert("连接已关闭...");
            ReConnect();
        };

    }
    else
        alert("您的浏览器不支持 WebSocket!");
}

//断线重连
function ReConnect() {
    setTimeout(InitWs, 5 * 1000);
}

//发送数据
function SendMsg(msg) {
    if (ws != null)
        ws.send("发送数据");
}

//关闭连接
function CloseWS() {
    if (ws != null)
        ws.close();
}