using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Warudo.Core;
using Warudo.Core.Attributes;
using Warudo.Core.Data;
using Warudo.Core.Resource;
using UnityEngine;
using Warudo.Core.Graphs;

namespace veasu.chathrow
{
  [NodeType(Id = "com.veasu.chatrthrownode", Title = "Get Prop From Message", Category = "CATEGORY_INTERACTIONS")]
  public class GetPropFromStringNode : Warudo.Core.Graphs.Node
  {

    [DataInput(23)]
    [Label("Message")]
    public string message;

    [DataInput(22)]
    [Label("COLLECTIONS")]
    [AutoComplete("AutoCompletePropCollection", true, "")]
    public string[] Collections;
    private Dictionary<string, string> choices = new Dictionary<string,string>();
    public async UniTask<AutoCompleteList> AutoCompletePropCollection() => AutoCompleteList.Single((IEnumerable<AutoCompleteEntry>) Context.ResourceManager.ProvideResources("Prop").ToAutoCompleteList().categories.Select<AutoCompleteCategory, AutoCompleteEntry>((Func<AutoCompleteCategory, AutoCompleteEntry>) (it => new AutoCompleteEntry()
    {
      label = it.LocalizedTitle,
      value = it.title
    })).ToList<AutoCompleteEntry>());

    [DataInput(24)]
    [Label("Max Props In Message")]
    public int maxProps = 3;

    [DataInput(21)]
    [Label("Timeout (seconds)")]
    [FloatSlider(0.0f, 60.0f)]
    public float timeoutAmount = 1.0f;

    [DataOutput(22)]
    [Label("Output Source")]
    public string getOutputData() {
      return outputData;
    }

    [FlowOutput]
    public Continuation OnPropFound;

    private string outputData = null;

    private float timer = 0.0f;

    static System.Random rnd = new System.Random();

    protected override void OnCreate() {
      Watch<string>(nameof(message), (from, to) => {
        if (to != null && choices.Count > 0 && Time.time > timer) {
          var lowerCaseMessage = to.ToLower();
          List<string> filteredChoices = new List<string>();
          foreach (string str in lowerCaseMessage.Split(' ')) {
            if (filteredChoices.Count >= maxProps) break;
            filteredChoices.AddRange(choices.Where(prop => str.Equals(prop.Key)).Select(it => it.Value));
          }
          if (filteredChoices.Count > 0) {
            outputData = filteredChoices[rnd.Next(filteredChoices.Count)];
            InvokeFlow(nameof(OnPropFound));
            timer = Time.time + timeoutAmount;
          }
          else {
            outputData = null;
          }
        }
      });

      Watch<float>(nameof(timeoutAmount), (from, to) => {
        timer -= (from - to);
      });

      Watch<string[]>(nameof(Collections),(from, to) => {
        AutoCompleteList autoCompleteList = Context.ResourceManager.ProvideResources("Prop").ToAutoCompleteList();
        this.choices.Clear();
        foreach (string collection1 in to)
        {
          AutoCompleteCategory completeCategory = autoCompleteList.categories.FirstOrDefault<AutoCompleteCategory>((Func<AutoCompleteCategory, bool>) (it => it.title == collection1));
          if (completeCategory != null) {
            var choicesDictOptions = completeCategory.entries.Select<AutoCompleteEntry, KeyValuePair<string,string>>((Func<AutoCompleteEntry, KeyValuePair<string,string>>) (it => new KeyValuePair<string,string>(it.value.Substring(it.value.LastIndexOf('/') + 1).ToLower(), it.value)));
            foreach (KeyValuePair<string,string> option in choicesDictOptions) {
              choices.Add(option.Key, option.Value);
            }
          }
        }
      });
    }

    public override void OnUpdate() {
    }
  }

}
