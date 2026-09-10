# On-Demand Production

Adds a third production mode to every production building. Click the Forever button to cycle Once, Maintain, and Forever. Maintain keeps a product in stock using this world's Resources list.

Inspired by RimWorld's "until you have X" bills. Vanilla Oxygen Not Included only lets a building finish a set count or run forever, so there is no way to hold a stock of meals, refined metal, or suits. The usual workaround is a Smart Storage Bin on automation: when the bin is full, the circuit disables the building. This mod puts that "keep this many" behavior on the production button itself.

![ALL DLC](https://raw.githubusercontent.com/canisminor1990/oni-mods/refs/heads/master/compatibility.png)

Steam Workshop: [按需生产](https://steamcommunity.com/sharedfiles/filedetails/?id=3797420331) · [Description.txt](Description.txt) (BBCode)

### Languages

UI text covers English, Chinese, Japanese, and Korean.

![Languages](https://raw.githubusercontent.com/canisminor1990/oni-mods/refs/heads/master/i18n.png)

### Features

- On any production building with a recipe queue, click the mode button next to the count.
- **Once**: produce the number in the field, then stop.
- **Maintain**: pause when this world's Resources list reaches that number, and start again when it drops below. Mass recipes can switch the field between kg and t (1 t = 1000 kg).
- **Forever**: keep producing.
- Totals come from the same world inventory as the resource overlay (reachable items on this asteroid or rocket, not the whole cluster).
- Materials use mass, suits and similar items use count — the same units as the Resources list. Food is counted by stored mass, which is one kilogram per item for most meals.
- Copy Building Settings copies Maintain.
- Buildings with no recipe list use the same count field, unit button, and mode button: Fertilizer Synthesizer, Compost, Power Control Station, and Farm Station. Fertilizer and dirt can switch kg and t (1 t = 1000 kg); microchips and farm kits are a count. Power Control Station and Farm Station cycle Auto and Maintain (vanilla Auto: craft only when a generator or plant needs the tool).

### Settings

- Open the Mods list, then click Settings on this mod.
- **Extra buildings** (on by default): one switch for Fertilizer Synthesizer, Compost, Power Control Station, and Farm Station. Off keeps those four vanilla. Recipe-queue buildings are unchanged.

### Update (1.1.0)

- Fertilizer Synthesizer, Compost, Power Control Station, and Farm Station now use the same Maintain controls as recipe-queue buildings.
- Power Control Station and Farm Station cycle Auto and Maintain (vanilla Auto: craft only when a generator or plant needs the tool).
- Mods list Settings adds one Extra buildings switch, on by default. Turn it off to keep those four vanilla.

### Feedback & Source

- Repository: [canisminor1990/oni-mods](https://github.com/canisminor1990/oni-mods)
- Bugs and requests go on [GitHub Issues](https://github.com/canisminor1990/oni-mods/issues). Please attach the full `Player.log` (`%USERPROFILE%/AppData/LocalLow/Klei/Oxygen Not Included/Player.log`).
- If you like the work and want to support it: [ko-fi.com/canisminor1990](https://ko-fi.com/canisminor1990)

---

# 按需生产 / On-Demand Production

*所有生产设备的生产模式改为点击切换：单次、维持、持续。维持会按本星球资源清单把库存做到设定数量。*

灵感来自 RimWorld 的「维持库存有 X 个」。原版缺氧的生产设备只能做完指定数量，或一直做下去，没有「维持这么多」——想囤一点食物、精炼金属或装备，往往会感到困惑。以前要维持库存，只能给智能储存箱接电路：箱子满了就断电停工，少了再开机。这个模组把同样的逻辑直接放进原来的模式按钮里。

![ALL DLC](https://raw.githubusercontent.com/canisminor1990/oni-mods/refs/heads/master/compatibility.png)

### 语言

界面文本覆盖英语、中文、日语和韩语。

![Languages](https://raw.githubusercontent.com/canisminor1990/oni-mods/refs/heads/master/i18n.png)

### 功能

- 所有带配方队列的生产设备都可以用，点数量旁边的模式按钮即可。
- **单次**：做完输入的数量后停止。
- **维持**：当前小行星资源清单达到该数量就停，低于后再继续。产物按质量计时，数量旁可以切换千克 / 吨（1 吨 = 1000 千克）。
- **持续**：一直生产。
- 数量来自原版资源统计（复制人能拿到的、本世界库存），不是整个星群。
- 材料按质量、装备等按个数，单位与资源清单一致。食物按储存质量计，大部分食物一件是 1 千克。
- 复制建筑设置会一并复制维持。
- 没有配方清单的建筑也用同一套数量框、单位按钮和模式按钮：肥料合成器、堆肥、电力控制站、农业站。肥料和泥土可以切换千克 / 吨（1 吨 = 1000 千克），微芯片和农业包按个。电力控制站和农业站只切换自动 / 维持（原版自动：只有发电机或植物需要时才做工具）。

### 设置

- 打开模组列表，点击本模组的「设置」。
- **额外建筑**（默认开）：一个总开关控制肥料合成器、堆肥、电力控制站、农业站。关掉后这四座保持原版。带配方队列的生产设备不受影响。

### 更新（1.1.0）

- 肥料合成器、堆肥、电力控制站、农业站也用同一套维持库存操作。
- 电力控制站和农业站只切换自动 / 维持（原版自动：只有发电机或植物需要时才做工具）。
- 模组列表的设置里增加「额外建筑」总开关，默认打开。关掉后这四座保持原版。

### 问题反馈与源码

- 模组仓库：[canisminor1990/oni-mods](https://github.com/canisminor1990/oni-mods)
- 问题请开 [GitHub Issues](https://github.com/canisminor1990/oni-mods/issues)。报告里务必附上完整的 `Player.log`（`%USERPROFILE%/AppData/LocalLow/Klei/Oxygen Not Included/Player.log`）。
- 喜欢这些模组、想支持一下：[ko-fi.com/canisminor1990](https://ko-fi.com/canisminor1990)
