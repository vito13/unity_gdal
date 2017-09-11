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
