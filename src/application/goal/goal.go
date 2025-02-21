package goal

import (
	"encoding/json"
)

/**
 * Go Authorization Library
 * Inspired by JS based CASL package
 */

type ActionType struct {
	key string
}

type actionsTypes struct {
	Read   ActionType
	Update ActionType
	Delete ActionType
	Create ActionType
}

var ActionTypes = actionsTypes{
	Read:   ActionType{key: "read"},
	Update: ActionType{key: "update"},
	Delete: ActionType{key: "delete"},
	Create: ActionType{key: "create"},
}

type AbilityRule struct {
	action  ActionType
	subject string
	params  *string
}

type AbilityBuilder struct {
	Abilities map[AbilityRule]bool
}

func makeAbilityRule(action ActionType, subject string, params *map[string]any) (AbilityRule, error) {
	var paramsData *string = nil

	if params != nil {
		data, err := json.Marshal(params)
		if err != nil {
			return AbilityRule{}, err
		}

		var str string = string(data)
		paramsData = &str
	}

	return AbilityRule{action: action, subject: subject, params: paramsData}, nil
}

func (b *AbilityBuilder) can(action ActionType, subject string, params *map[string]any) error {
	key, err := makeAbilityRule(action, subject, params)

	if err != nil {
		return err
	}

	b.Abilities[key] = true
	return nil
}

type GoAL struct {
}

func (g *GoAL) GetAbilityBuilder() AbilityBuilder {
	return AbilityBuilder{}
}

func (g *GoAL) FinalizeAbility(builder AbilityBuilder) Ability {
	return Ability{abilities: builder.Abilities}
}

type Ability struct {
	abilities map[AbilityRule]bool
}

func (a *Ability) Can(action ActionType, subject string, params *map[string]any) (bool, error) {
	ability, err := makeAbilityRule(action, subject, params)
	if err != nil {
		return false, err
	}

	_, exists := a.abilities[ability]
	if !exists {
		return false, nil
	} else {
		return true, nil
	}
}

// Usage example

var g = GoAL{}

var builder AbilityBuilder = g.GetAbilityBuilder()

func Gtet() {
	builder.can(ActionTypes.Update, "Channel", nil)
	builder.can(ActionTypes.Read, "Channel", nil)
	builder.can(ActionTypes.Delete, "Channel", nil)
	builder.can(ActionTypes.Create, "Channel", nil)

	var abil = g.FinalizeAbility(builder)

	println(abil.Can(ActionTypes.Read, "Channel", nil))
}
