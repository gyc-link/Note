
该笔记主要参考[git使用规范](../resources/git使用规范.pdf)和[git管理方案范本](../resources/git管理方案范本.pdf)

sj项目

分支命名规范

开发者临时分支
name/develop
确认不需要后及时清理

基于master，软件准备销售给客户的每一个版本均需要一个发行分支。

版本标签以release 或 client分支为基础
版本标签与软件代码内的版本号保持一致
命名：

commit
原子化提交、提交说明规范、保证软件可编译并且能执行

feat 新增feature
fix 修复bug
docs 修改文档
style 不改变代码逻辑，仅修改代码格式
refactor 代码重构
test 测试用例
chore 改变构建流程、增加依赖库、工具等
revert 回滚到上一个版本
save 为了保存更改发起的临时提交，后续不因该出现在提交历史中

以上是type

< type > :< subject >简短描述，结尾不加句号，动词开头 change 之类的单词，第一人称现在时

< body > 详细描述

为什么变更是必须的
如何解决这个问题，步骤是什么
是否存在其他不好的影响？

可选：添加一个链接到issue

最后 Footer只用于以下两种情况
不兼容变动
如果当前代码与上一个版本不兼容，以BREAKING CHANGE开头
后面对变动的描述
关闭issue
如果当前commit针对某个issue，可以在footer部分关闭这个issue



坚持上游优先原则 uostream first
严禁在下游直接打补丁，
现在主干上开fix分支，修复合并后，切换到下游分支，在git cherry pick 之后打包发版

rebase只能在本地私有开发分支上使用

冲突处理切勿 Commit：Rebase 遇到冲突时，解决冲突后执⾏ git add . 然后直接执⾏ git rebase --continue 。千万 不要去执⾏ Commit，否则会打断变基流程。

冲突解决sop
本地解决原则
冲突处理一定要本地验证

squash往往在本地，连续的，做的是一件事儿

编译的时候用git的标签指定