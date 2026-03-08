using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timesystem : MonoBehaviour
{
    // 时间显示 // 24分钟为一天
    public Text year;
    public int yearTime = 2148;
    public Text TimeShow;
    // 季节切换
    public Text moon;
    // 节日切换
    public Text holiday;
    // 天气
    public Text weather;
    public Text day;
    // 累加时间
    public float totalSeconds = 0;
    // 分钟,初始进游戏为08：00
    public int showMinute = 0;
    // 秒钟
    public int showSeconds = 0;
    // 季节切换判断,1春天，2夏天，3秋天，4冬天
    public int moonTime = 1;
    // 日期，一个月30，一个月一个季节
    public int dayTime = 30;
    // Start is called before the first frame update
    void Start()
    {

    }
    private void FixedUpdate()
    {
        // 累加时间到1秒，然后重置秒钟为0（每秒钟执行一次）
        totalSeconds += 60* Time.fixedDeltaTime;
        if (totalSeconds >= 0.98)
        {

            UpdateTime();
            UpdateDayAndMoon();
            totalSeconds = 0;
        }

    }

    private void Update()
    {

    }

    // 更新时间
    public void UpdateTime()
    {
        // 判断分钟是否为23
        if (showMinute == 23)
        {
            // 判断秒钟是否为59
            if (showSeconds == 59)
            {
                // 重置分钟和秒钟为0
                showMinute = 0;
                showSeconds = 0;
                //界面显示时间为凌晨
                TimeShow.text = "00:00";
            }
            else
            {
                // 秒钟累加
                showSeconds += 1;
                //判断秒钟位数是否为2，为1在前面加零
                if (showSeconds.ToString().Length == 1)
                {
                    string seconds = "0" + showSeconds.ToString();
                    TimeShow.text = "23:" + seconds;
                }
                else
                {
                    TimeShow.text = "23:" + showSeconds.ToString();
                }
            }
        }
        else
        {
            // print("分钟:"+showMinute+",秒钟:"+showSeconds);
            // 当时间不为23分钟时
            //判断秒钟是否为59秒
            if (showSeconds == 59)
            {
                // 重置秒钟为0；
                showSeconds = 0;
                // 分钟+1；
                showMinute += 1;
                // 判断分钟是否为2位，1位补0
                if (showMinute.ToString().Length == 1)
                {
                    // 将时间显示在界面
                    TimeShow.text = "0" + showMinute.ToString() + ":00";
                }
                else
                {
                    // 将时间显示在界面
                    TimeShow.text = showMinute.ToString() + ":00";
                }


            }
            else
            {
                // 当秒钟小于59
                // 秒钟++
                showSeconds += 1;
                //判断秒钟位数

                // 保存分钟
                string minutes = showMinute.ToString();
                // 保存秒钟
                string seconds = showSeconds.ToString();

                // 分钟一位的时候补零
                if (minutes.Length == 1)
                {
                    minutes = "0" + showMinute.ToString();
                }
                // 秒钟一位补0
                if (seconds.Length == 1)
                {
                    seconds = "0" + showSeconds.ToString();
                }
                // 将时间显示在界面
                TimeShow.text = minutes + ":" + seconds;
            }
        }

    }

    // 更新日期和季节
    public void UpdateDayAndMoon()
    {
        // 判断是否为00：00(即代表)，这里代码只有当第二天的时候才会执行
        if (showMinute == 0 && showSeconds == 0)
        {
            // 判断是否是春天
            if (moonTime == 1)
            {
                // 判断日期是否是30
                if (dayTime == 30)
                {
                    // 重置日期，为了调用下一个季节代码准备
                    dayTime = 0;
                    // 调整月份
                   moonTime +=1;
                  
                   if(moonTime>12)
                    {
                        moonTime = 1;
                        yearTime += 1;
                    }
                    year.text = yearTime.ToString() + "年";
                    moon.text = moonTime.ToString() + "月";
                    // 更新日期到界面，
                    day.text = "1日";
                }
                else
                {
                    // 日期++
                    dayTime += 1;
                    // 更新日期到界面
                    day.text = dayTime.ToString() + "日";
                }
            }


    }

    // 更新节日



    }

}
