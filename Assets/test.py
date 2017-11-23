import clr
clr.AddReferenceByPartialName('UnityEngine')
import UnityEngine

import sys
sys.path.append(UnityEngine.Application.dataPath + '/plugins/IronPython2.7/Lib')

import datetime


def print_message():
    UnityEngine.Debug.Log('Test message from Python!')
def p2():
    UnityEngine.Debug.Log(datetime.datetime.today())
def say_hello():
    print("hello!")



# -*- coding: utf-8 -*-
import matplotlib.pyplot as plt
import matplotlib
zhfont1 = matplotlib.font_manager.FontProperties(fname='c:\\msyh.ttc')
plt.xlabel(u"横坐标xlabel",fontproperties=zhfont1)

from matplotlib import pyplot as plt
years = [1950, 1960, 1970, 1980, 1990, 2000, 2010] 
gdp = [300.2, 543.3, 1075.9, 2862.5, 5979.6, 10289.7, 14958.3] 
 
# 创建一幅线图， x轴是年份， y轴是gdp 
plt.plot(years, gdp, color='green', marker='o', linestyle='solid') 
 
# 添加一个标题 
plt.title(u"名义GDP",fontproperties=zhfont1) 
 
# 给y轴加标记 
plt.ylabel(u"十亿美元",fontproperties=zhfont1) 
plt.show()
