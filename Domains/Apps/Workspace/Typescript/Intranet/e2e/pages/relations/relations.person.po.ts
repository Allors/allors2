import { $, browser, by, element, ExpectedConditions } from "protractor";
import { WebElementPromise } from "selenium-webdriver";
import { BasePage } from "../base.po";
import { config } from "../config";

export class RelationsPersonPage extends BasePage {

  public async navigateTo(id: string): Promise<any> {
    const url = `/relations/person/${id}`;
    await this.navigate(url);
  }

  get FirstName(): WebElementPromise {
    return browser.findElement(by.css("input[placeholder='First Name']"));
  }
}
