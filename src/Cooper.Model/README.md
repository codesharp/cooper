# Cooper核心业务模型

此项目描述严谨的核心业务模型，严格面向业务设计，
外围会有api层提供各类型DTO模型来解决查询、远程调用的问题，
事务性操作回归此模型层

===

## 规范

- 按目录划分聚合
- 按命名空间划分模块
- 充血模型 DDD
- 使用Assert进行Model层所有的断言和参数检查，不可出现任何独立throw的exception和中文/英文异常内容

## 主要聚合/模型


### Task

专属于Cooper的任务模型，扩展性等设计均建立在Cooper的账号体系之上，不考虑用于描述通用任务模型

### TaskFolder

- personal
- team
- project

### Account
以“人”为中心的Cooper账号体系，用于支持广泛的外部连接以及联系人关联

- AccountConnection
	- Google
	- Git
	- Weibo

### Contact

存在于Contacts通讯录中的联系人信息，汇聚为账号之间的联系人“池”，用于支持跨组织的设计

- Contact
	
- AddressBook
	- Personal
	- ENT/Global
- ContactGroup

### Team

成员由Contact组成，one-to-many Project

Team <--> ContactGroup

### Project


### Relationship

- task access
	- [X] account -> task
	- [X] account -> folder -> task
	- [X] account -> tag -> task

	- [ ] contact -> task (assigned)
	- [-] contact -> account -> folder ? (personal overview)
	
	- [ ] team -> contact -> task
	- [ ] team -> tag -> task (filter)
	- [-][C] team -> folder ->task
	
	- [ ] project -> task
	- [-][C] project -> folder -> task

- only directly access
	- tag -> task
	- project -> task

[X]: Done
[-]: Later
[C]: Canceled

### Access Control