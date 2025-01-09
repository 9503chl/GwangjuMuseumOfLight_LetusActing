using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Calculator : MonoBehaviour
{
    public string CurrentText;

    private string beforeText;

    private bool isOperate = false;

    private OperationType operationType = OperationType.Default;

    private void OnEnable()
    {
        //currentMoneyText.text = "0";
        CurrentText = null;
        beforeText = null;
        isOperate = false;
        operationType = OperationType.Default;
    }

    private void InvokeNumber(int number)
    {
        CurrentText += number.ToString();

        if (isOperate)
        {
            int temp = Convert.ToInt32(beforeText);

            switch (operationType)
            {
                case OperationType.Plus:
                    temp += number;
                    break;
                case OperationType.Minus:
                    temp -= number;
                    break;
                case OperationType.Multiply:
                    temp *= number;
                    break;
                case OperationType.Divide:
                    temp /= number;
                    break;
            }

            CurrentText = temp.ToString();
            beforeText = string.Empty;
            isOperate = false;
        }
        //currentMoneyText.text = string.Format("{0}", Convert.ToInt32(CurrentText));
    }
    private void Operation(int type)
    {
        beforeText = CurrentText;

        switch (type)
        {
            case 0:
                operationType = OperationType.Plus;
                isOperate = true;
                break;
            case 1:
                operationType = OperationType.Minus;
                isOperate = true;
                break;
            case 2:
                operationType = OperationType.Multiply;
                isOperate = true;
                break;
            case 3:
                operationType = OperationType.Divide;
                isOperate = true;
                break;

            case 4://왜 한번만 될꼬
                if (isOperate)
                {
                    operationType = OperationType.Default;
                    isOperate = false;
                }
                else
                {
                    try
                    {
                        CurrentText = CurrentText.Substring(0, CurrentText.Length - 1);
                        //currentMoneyText.text = string.Format("{0}", Convert.ToInt32(CurrentText));
                    }
                    catch
                    {
                        CurrentText = "0";
                        //currentMoneyText.text = CurrentText;
                    }
                }
                beforeText = string.Empty;
                break;
            case 5:
                operationType = OperationType.Default;

                isOperate = false;

                CurrentText = "0";
                beforeText = null;

                //currentMoneyText.text = CurrentText;
                break;
        }
    }
}
