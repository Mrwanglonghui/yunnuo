layui.use('layim', function (layim) {
    //基础配置
    layim.config({
        //初始化接口
        init: {
            url: 'json/getList.json',
            data: {},
            
        },
        copyright:true,
        min:false,
        brief:false,
        title:'webIM',
    });
    // layim.cache().base.title = "消息";//聊天框缩小后默认显示的文字
})