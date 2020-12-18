var ws = null;
var _src = null;

//��ʼ��websocket
function InitWs() {
    if ("WebSocket" in window) {
        // ��һ�� web socket
        ws = new WebSocket(_src);

        //��
        ws.onopen = function () {
            // Web Socket �������ϣ�ʹ�� send() ������������

            alert("���ݷ�����...");
        };

        //�յ���Ϣ
        ws.onmessage = function (evt) {
            var received_msg = evt.data;
            alert("�����ѽ���...");
        };

        //�ر�
        ws.onclose = function () {
            // �ر� websocket
            alert("�����ѹر�...");
            ReConnect();
        };

    }
    else
        alert("�����������֧�� WebSocket!");
}

//��������
function ReConnect() {
    setTimeout(InitWs, 5 * 1000);
}

//��������
function SendMsg(msg) {
    if (ws != null)
        ws.send("��������");
}

//�ر�����
function CloseWS() {
    if (ws != null)
        ws.close();
}