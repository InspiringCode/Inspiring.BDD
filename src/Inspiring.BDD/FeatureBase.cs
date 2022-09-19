using Inspiring.BDD.Core;

namespace Inspiring.BDD {
    public class FeatureBase : LightBddFeature {
        protected ScenarioStepFactory USING { get; }
        protected ScenarioStepFactory GIVEN { get; }
        protected ScenarioStepFactory WHEN { get; }
        protected ScenarioStepFactory THEN { get; }
        protected ScenarioStepFactory AND { get; }

        public FeatureBase() {
            USING = new ScenarioStepFactory(Scenario, "USING ");
            GIVEN = new ScenarioStepFactory(Scenario, "GIVEN ");
            WHEN = new ScenarioStepFactory(Scenario, "WHEN ");
            THEN = new ScenarioStepFactory(Scenario, "THEN ");
            AND = new ScenarioStepFactory(Scenario, "AND ");
        }
    }
}
