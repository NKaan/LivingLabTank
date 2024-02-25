using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace SL.Wait
{
  public class WaitTest : MonoBehaviour
  {
    public UIDocument rootDocument;
    public static bool triggered = false;
    public static bool chained = false;

    private void Start()
    {
      VisualElement root = rootDocument.rootVisualElement;

      StyleSheet styles =
        Resources.Load<StyleSheet>("Styles");

      root.styleSheets.Add(styles);

      root.Add(TestUI(root));
    }

    private VisualElement TestUI(VisualElement root)
    {
      VisualElement container = new VisualElement();
      container.AddToClassList("container");

      VisualElement leftColumn = new VisualElement();
      leftColumn.AddToClassList("left-column");
      VisualElement rightColumn = new VisualElement();
      rightColumn.AddToClassList("right-column");

      leftColumn.Add(WaitBuilder(root));
      leftColumn.Add(WaitBuilder(root, "Chain"));
      leftColumn.Add(WaitActions(root));

      TextField resultsField = new TextField { name = "results", multiline = true };

      rightColumn.Add(resultsField);

      container.Add(leftColumn);
      container.Add(rightColumn);

      return container;
    }

    private void PushMessage(VisualElement root, string message)
    {
      TextField resultsField = root.Query<TextField>("results");

      string hour = DateTime.Now.Hour < 10
        ? $"0{DateTime.Now.Hour}"
        : $"{DateTime.Now.Hour}";
      string minute = DateTime.Now.Minute < 10
        ? $"0{DateTime.Now.Minute}"
        : $"{DateTime.Now.Minute}";
      string second = DateTime.Now.Second < 10
        ? $"0{DateTime.Now.Second}"
        : $"{DateTime.Now.Second}";

      resultsField.value += $"\n[{hour}:{minute}:{second}] {message}";
    }

    private VisualElement WaitBuilder(VisualElement root, string builderName = "")
    {
      VisualElement builder = new VisualElement { name = builderName };
      builder.AddToClassList("builder");

      VisualElement chainLabel = new VisualElement();
      if (builderName == "Chain") {
        builder.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);

        chainLabel = new Label("Chain");
        chainLabel.AddToClassList("wait-label");
      }

      Label waitLabel = new Label("Wait");
      waitLabel.AddToClassList("wait-label");

      TextField durationField = new TextField { value = "3" };
      durationField.RegisterCallback<ChangeEvent<string>>(
        changeEvent => {
          float newValue;
          float.TryParse(changeEvent.newValue, out newValue);
          durationField.value = $"{newValue}";
        }
      );
      durationField.AddToClassList("duration-field");

      RadioButtonGroup typeSelect    = new RadioButtonGroup();
      RadioButton      seconds       = new RadioButton("Seconds") { name         = "seconds", value = true };
      RadioButton      frames        = new RadioButton("Frames") { name          = "frames" };
      RadioButton      secondsPaused = new RadioButton("Seconds Paused") { name  = "secondsPaused" };
      RadioButton      until         = new RadioButton("Until Triggered") { name = "until" };

      typeSelect.AddToClassList("type-select");
      seconds.AddToClassList("type");
      frames.AddToClassList("type");
      secondsPaused.AddToClassList("type");
      until.AddToClassList("type");

      typeSelect.Add(seconds);
      typeSelect.Add(frames);
      typeSelect.Add(secondsPaused);
      typeSelect.Add(until);

      Label thenLabel = new Label("Then output");
      waitLabel.AddToClassList("wait-label");

      TextField outputField = new TextField { value = "Finished!" };
      outputField.AddToClassList("output-field");

      Label tagLabel = new Label("Tag as");
      waitLabel.AddToClassList("wait-label");

      TextField tagField = new TextField { value = $"{builderName}Wait" };
      tagField.AddToClassList("tag-field");

      Button runButton = new Button(
        () => {
          Wait   timing     = new Wait();
          string output     = outputField.value;
          string tagName    = tagField.value;
          float  duration   = float.Parse(durationField.value);
          string timingName = $"{duration} Seconds";

          if (frames.value) {
            timing.ForFrames(duration);
            timingName = $"{duration} Frames";
          } else if (secondsPaused.value) {
            timing.ForSecondsByFrame(duration);
            timingName = $"{duration} Seconds Paused";
          } else if (until.value) {
            timing.Until(
              () => triggered
            );
            timingName = "Until Triggered";
          } else {
            timing.ForSeconds(duration);
          }

          timing.Then(
            () => {
              PushMessage(root, $"{tagName} - {output}");
              if (triggered) {
                triggered = false;
              }
            }
          );

          if (chained) {
            VisualElement chainElement       = root.Query<VisualElement>("Chain");
            TextField     chainOutputField   = chainElement.Query<TextField>(null, "output-field");
            TextField     chainTagField      = chainElement.Query<TextField>(null, "tag-field");
            TextField     chainDurationField = chainElement.Query<TextField>(null, "duration-field");

            chainDurationField.RegisterCallback<ChangeEvent<string>>(
              changeEvent => {
                float newValue;
                float.TryParse(changeEvent.newValue, out newValue);
                chainDurationField.value = $"{newValue}";
              }
            );
            
            // RadioButton chainSeconds    = chainElement.Query<RadioButton>("seconds");
            RadioButton chainFrames        = chainElement.Query<RadioButton>("frames");
            RadioButton chainSecondsPaused = chainElement.Query<RadioButton>("secondsPaused");
            RadioButton chainUntil         = chainElement.Query<RadioButton>("until");

            Wait   chain           = new Wait();
            string chainOutput     = chainOutputField.value;
            string chainTagName    = chainTagField.value;
            float  chainDuration   = float.Parse(chainDurationField.value);
            string chainTimingName = $"{chainDuration} Seconds";

            if (chainFrames.value) {
              chain.ForFrames(chainDuration);
              chainTimingName = $"{chainDuration} Frames";
            } else if (chainSecondsPaused.value) {
              chain.ForSecondsByFrame(chainDuration);
              chainTimingName = $"{chainDuration} Seconds Paused";
            } else if (chainUntil.value) {
              chain.Until(
                () => triggered
              );
              chainTimingName = "Until Triggered";
            } else {
              chain.ForSeconds(chainDuration);
            }

            chain.Then(
              () => {
                PushMessage(root, $"{chainTagName} - {chainOutput}");
                if (triggered) {
                  triggered = false;
                }
              }
            );
            chain.OnStart(
              () => {
                PushMessage(root, $"Started {chainTagName} - {chainTimingName}");
              }
            );

            timing.Chain(chain);
          }

          PushMessage(root, $"Started {tagName} - {timingName}");
          timing.Start();
        }
      ) { text = "Start" };
      runButton.AddToClassList("run-button");

      builder.Add(chainLabel);
      builder.Add(waitLabel);
      builder.Add(durationField);
      builder.Add(typeSelect);
      builder.Add(thenLabel);
      builder.Add(outputField);
      builder.Add(tagLabel);
      builder.Add(tagField);
      if (builderName != "Chain") {
        builder.Add(runButton);
      }

      return builder;
    }

    private VisualElement WaitActions(VisualElement root)
    {
      VisualElement actions = new VisualElement();
      actions.AddToClassList("actions");

      Button pauseButton = new Button();
      pauseButton.clicked +=
        () => {
          if (Math.Abs(Time.timeScale - 1) < 0.01) {
            Time.timeScale   = 0;
            pauseButton.text = "Paused";
          } else {
            Time.timeScale   = 1;
            pauseButton.text = "Running (click to pause)";
          }
        };
      pauseButton.text = "Running (click to pause)";
      pauseButton.AddToClassList("action-button");

      Button untilTriggerButton = new Button(
        () => {
          triggered = true;
        }
      ) { text = "Trigger Until" };
      untilTriggerButton.AddToClassList("action-button");

      Button addChainButton = new Button(
        () => {
          VisualElement chain = root.Query<VisualElement>("Chain");
          if (chain != null) {
            chained = !chained;
            chain.style.display = chained
              ? new StyleEnum<DisplayStyle>(DisplayStyle.Flex)
              : new StyleEnum<DisplayStyle>(DisplayStyle.None);
          }
        }
      ) { text = "Toggle Chain" };
      addChainButton.AddToClassList("action-button");

      actions.Add(pauseButton);
      actions.Add(addChainButton);
      actions.Add(untilTriggerButton);

      return actions;
    }
  }
}
