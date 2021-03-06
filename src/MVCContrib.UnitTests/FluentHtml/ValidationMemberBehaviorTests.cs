using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Web.Mvc;
using MvcContrib.FluentHtml.Behaviors;
using MvcContrib.FluentHtml.Elements;
using MvcContrib.FluentHtml.Expressions;
using MvcContrib.UnitTests.FluentHtml.Fakes;
using MvcContrib.UnitTests.FluentHtml.Helpers;
using NUnit.Framework;
using HtmlAttribute=MvcContrib.FluentHtml.Html.HtmlAttribute;

namespace MvcContrib.UnitTests.FluentHtml
{
    [TestFixture]
	public class ValidationMemberBehaviorTests
	{
		ModelStateDictionary stateDictionary;
		private Expression<Func<FakeModel, object>> expression;

		[SetUp]
		public void SetUp()
		{
			stateDictionary = new ModelStateDictionary();
		    expression = null;
		}

		[Test]
		public void element_for_member_with_no_error_renders_with_no_class()
		{
			var target = new ValidationBehavior(() => stateDictionary);
			expression = x => x.Price;
			var textbox = new TextBox(expression.GetNameFor(), null, new List<IBehaviorMarker> { target });
			var element = textbox.ToString().ShouldHaveHtmlNode("Price");
			element.ShouldNotHaveAttribute(HtmlAttribute.Class);
		}

		[Test]
		public void element_for_member_with_error_renders_with_default_error_class()
		{
			stateDictionary.AddModelError("Price", "Something bad happened");
			var target = new ValidationBehavior(() => stateDictionary);
			expression = x => x.Price;
			var textbox = new TextBox(expression.GetNameFor(), null, new List<IBehaviorMarker> { target });
			var element = textbox.ToString().ShouldHaveHtmlNode("Price");
			element.ShouldHaveAttribute(HtmlAttribute.Class).WithValue("input-validation-error");
		}

		[Test]
		public void element_for_member_with_error_renders_with_specified_error_class_and_specified_other_class()
		{
			stateDictionary.AddModelError("Price", "Something bad happened");
			var target = new ValidationBehavior(() => stateDictionary, "my-error-class");
			expression = x => x.Price;
			var textbox = new TextBox(expression.GetNameFor(), null, new List<IBehaviorMarker> { target })
				.Class("another-class");
			var element = textbox.ToString().ShouldHaveHtmlNode("Price");
			element.ShouldHaveAttribute(HtmlAttribute.Class).Value
				.ShouldContain("another-class")
				.ShouldContain("my-error-class");
		}

		[Test]
		public void element_with_error_renders_with_attempted_value()
		{
			stateDictionary.AddModelError("Price", "Something bad happened");
		    stateDictionary["Price"].Value = new ValueProviderResult("bad value", "bad value", CultureInfo.CurrentCulture);
			var target = new ValidationBehavior(() => stateDictionary);
			expression = x => x.Price;
			var textbox = new TextBox(expression.GetNameFor(), expression.GetMemberExpression(),
				new List<IBehaviorMarker> { target });
			var element = textbox.ToString().ShouldHaveHtmlNode("Price");
			element.ShouldHaveAttribute(HtmlAttribute.Value).WithValue("bad value");
		}

		[Test]
    	public void element_without_error_renders_with_attempted_value()
    	{
    		stateDictionary.Add("Price", new ModelState() { Value = new ValueProviderResult("foo", "foo", CultureInfo.CurrentCulture) });

			var target = new ValidationBehavior(() => stateDictionary);
			expression = x => x.Price;
			var textbox = new TextBox(expression.GetNameFor(), expression.GetMemberExpression(), new List<IBehaviorMarker> { target });
			var element = textbox.ToString().ShouldHaveHtmlNode("Price");
			element.ShouldHaveAttribute(HtmlAttribute.Value).WithValue("foo");

    	}

    	[Test]
    	public void does_not_add_css_class_when_retrieving_value_from_modelstate_with_no_error()
    	{
			stateDictionary.Add("Price", new ModelState() { Value = new ValueProviderResult("foo", "foo", CultureInfo.CurrentCulture) });
			
			var target = new ValidationBehavior(() => stateDictionary);
			expression = x => x.Price;
			var textbox = new TextBox(expression.GetNameFor(), null, new List<IBehaviorMarker> { target });
			var element = textbox.ToString().ShouldHaveHtmlNode("Price");

			element.ShouldHaveAttribute(HtmlAttribute.Value).WithValue("foo");
			element.ShouldNotHaveAttribute(HtmlAttribute.Class);
    	}

		[Test]
		public void handles_checkboxes_correctly()
		{
			stateDictionary.AddModelError("Done", "Foo");
			stateDictionary["Done"].Value = new ValueProviderResult(new[] { "true", "false" }, "true", CultureInfo.CurrentCulture);
			var target = new ValidationBehavior(() => stateDictionary);
			expression = x => x.Done;
			var checkbox = new CheckBox(expression.GetNameFor(), expression.GetMemberExpression(), new List<IBehaviorMarker> { target });
			var element = checkbox.ToString().ShouldHaveHtmlNode("Done");
			element.ShouldHaveAttribute("checked").WithValue("checked");
			element.ShouldHaveAttribute("value").WithValue("true");
		}

		[Test]
		public void when_handling_checkbox_does_not_fall_back_to_default_behavior()
		{
			stateDictionary.AddModelError("Done", "Foo");
			stateDictionary["Done"].Value = new ValueProviderResult(new[] { "false", "false" }, "false", CultureInfo.CurrentCulture);
			var target = new ValidationBehavior(() => stateDictionary);
			expression = x => x.Done;
			var checkbox = new CheckBox(expression.GetNameFor(), expression.GetMemberExpression(), new List<IBehaviorMarker> { target });
			var element = checkbox.ToString().ShouldHaveHtmlNode("Done");
			element.ShouldHaveAttribute("value").WithValue("true");
		}

    	[Test]
    	public void does_not_restore_value_for_password_field()
    	{
			stateDictionary.Add("Password", new ModelState() { Value = new ValueProviderResult("foo", "foo", CultureInfo.CurrentCulture) });

			var target = new ValidationBehavior(() => stateDictionary);
			expression = x => x.Password;
			var passwordField = new Password(expression.GetNameFor(), expression.GetMemberExpression(), new List<IBehaviorMarker> { target });
			var element = passwordField.ToString().ShouldHaveHtmlNode("Password");
			element.ShouldHaveAttribute(HtmlAttribute.Value).WithValue("");
    	}
	}
}