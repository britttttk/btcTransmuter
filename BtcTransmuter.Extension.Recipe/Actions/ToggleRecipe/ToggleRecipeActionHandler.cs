using System;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.Helpers;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Extension.Recipe.Actions.CreateRecipe;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace BtcTransmuter.Extension.Recipe.Actions.ToggleRecipe
{
    public class ToggleRecipeActionHandler : BaseActionHandler<ToggleRecipeData>
    {
        public override string ActionId => "ToggleRecipe";
        public override string Name => "Toggle Recipe";

        public override string Description =>
            "Enable/Disable a recipe within the system";

        public override string ViewPartial => "ViewToggleRecipeAction";

        public override string ControllerName => "ToggleRecipe";

        protected override Task<bool> CanExecute(object triggerData, RecipeAction recipeAction)
        {
            return Task.FromResult(recipeAction.ActionId == ActionId);
        }

        protected override async Task<ActionHandlerResult> Execute(object triggerData, RecipeAction recipeAction,
            ToggleRecipeData actionData)
        {
            try
            {
                using (var scope = DependencyHelper.ServiceScopeFactory.CreateScope())
                {
                    var recipeManager = scope.ServiceProvider.GetService<IRecipeManager>();
                    var recipe = await recipeManager.GetRecipe(actionData.TargetRecipeId);
                    if (recipe == null)
                    {
                        return new ActionHandlerResult()
                        {
                            Executed = false,
                            Result =
                                $"Could not find recipe to toggle"
                        };
                    }

                    switch (actionData.Option)
                    {
                        case ToggleRecipeData.ToggleRecipeOption.Enable:
                            recipe.Enabled = true;
                            break;
                        case ToggleRecipeData.ToggleRecipeOption.Disable:
                            recipe.Enabled = false;
                            break;
                        case ToggleRecipeData.ToggleRecipeOption.Toggle:
                            recipe.Enabled = !recipe.Enabled;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    await recipeManager.AddOrUpdateRecipe(recipe);
                    return new ActionHandlerResult()
                    {
                        Executed = true,
                        Result =
                            $"Recipe {recipe.Name} is now {(recipe.Enabled ? "Enabled" : "Disabled")}"
                    };
                }
            }
            catch (Exception e)
            {
                return new ActionHandlerResult()
                {
                    Executed = false,
                    Result =
                        $"Could not toggle recipe because {e.Message}"
                };
            }
        }
    }
}