# SubtitleSupporter

v0.6

这个项目是Aegisub的一个lua脚本，可以实现调用whipser api，压制视频等功能，减轻翻译工作负担。

目前功能还不完善，更多的是想起一个抛砖引玉的效果，提供一个通过lua脚本调用本地python，c#等程序，然后处理返回的字符串结果的思路。可以规避掉lua开发的复杂性和功能的局限性，尽可能的为翻译和打轴提供更多帮助。

・安装说明
[松田好花花AE字幕小助手安装说明.docx](https://github.com/magictp/SubtitleSupporter/files/11121170/AE.docx)


・目前功能
1. 对有原生字幕的视频进行自动打轴(Get Current Subtitle)

https://user-images.githubusercontent.com/8610724/229104538-4db6232d-971a-41ef-b5d4-eb0faf369b20.mp4

原理是通过设置原生字幕的起始和终了坐标，当原生字幕范围内图像变动大于设定的置信值(confidence)时，记录下当前帧数。

对于原生字幕背景固定的视频效果非常好，但是对于背景变动的视频效果欠佳。

目前的优化思路是追加文字识别功能，对于识别出相同内容的字幕进行合并，但是还没有找到合适的ocr程序。


2. 调用whisper api进行自动打轴(Run Whisper Api)

https://user-images.githubusercontent.com/8610724/229105656-bad58eb1-b1a8-4d58-8994-885676b75a5e.mp4

需要事前在config文件中设置whisper的key，通过vpn连接时偶尔会出现返回结果为空的情况，可以切换节点或挑选其他时间尝试。

是用http请求进行访问的，不需要本地配置任何环境。

通过ffmpeg切割视频，也解决了单次25m的文件限制，但是在切割点的解析效果会比较差。

3. 压制视频（Combine Video with Subtitle）

4. 调用本地whisper进行自动打轴(Run Whisper Local)

对于本地配置完whisper环境的用户，可以通过修改lua文件，打开调用本地whisper的功能。




・异常调试

lua脚本同层目录内会有ss.log文件(如果aegisub安装在program files目录下，需要用管理员权限执行才会生成log文件)，里面会有更具体的错误信息。



⁂感谢 困惑塩 提供建议，帮助测试和编写安装说明

⁂感谢 凳子 帮助测试

