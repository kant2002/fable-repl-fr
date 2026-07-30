module WebComponent

// Web Components avec Fable by Onur Gümüş (Twitter @OnurGumusDev)
// Check the custom tag dans the HTML tab et read this thread pour more info:
// https://twitter.com/OnurGumusDev/status/1329019698667790337

// For a more high-level library à create Web Components, essayer Fable.Lit:
// https://fable.io/Fable.Lit/docs/web-components.html

ouvrir Fable.Core
ouvrir Browser
ouvrir Browser.Types
ouvrir Fable.Core.JsInterop

[<AllowNullLiteral>]
taper HTMLTemplateElement =
    hérites HTMLElement
    abstraite content: DocumentFragment avec get, set

[<AllowNullLiteral>]
taper HTMLTemplateElementType =
    [<EmitConstructor>]
    abstraite Create: unit -> HTMLTemplateElement

laisser template: HTMLTemplateElement =
    abattue document.createElement ("template")

template.innerHTML <-
    """
  <style>
    .container {
      padding: 8px;
    }
    button {
      display: block;
      overflow: hidden;
      position: relative;
      padding: 0 16px;
      font-size: 16px;
      font-weight: bold;
      text-overflow: ellipsis;
      white-space: nowrap;
      cursor: pointer;
      outline: none;
      width: 100%;
      height: 40px;
      box-sizing: border-box;
      border: 1px solid #a1a1a1;
      background: #ffffff;
      box-shadow: 0 2px 4px 0 rgba(0,0,0, 0.05), 0 2px 8px 0 rgba(161,161,161, 0.4);
      color: #363636;
      cursor: pointer;
    }
  </style>
  <div classe="container">
    <button>Label</button>
  </div>
"""

[<Global>]
module customElements =
    laisser define (elementName: string, ty: obj) = jsNative

[<Global>]
taper ShadowRoot() =
    membre this.appendChild(el: Browser.Types.Node) = jsNative
    membre this.querySelector(selector: string): Browser.Types.HTMLElement = jsNative

laisser enligne attachStatic<'T> (name: string) (f: obj): unit = jsConstructor<'T>?name <- f

laisser enligne attachStaticGetter<'T, 'V> (name: string) (f: unit -> 'V): unit =
    JS.Constructors.Object.defineProperty (jsConstructor<'T>, name, !!{| get = f |})
    |> ignore

[<Global; AbstractClass>]
[<AllowNullLiteral>]
taper HTMLElement() =
    membre _.getAttribute(attr: string): string = jsNative
    membre _.attachShadow(obj): ShadowRoot = jsNative
    abstraite connectedCallback: unit -> unit
    abstraite attributeChangedCallback: string * obj * obj -> unit

[<AllowNullLiteral>]
taper Button() =
    hérites HTMLElement()

    laisser shadowRoot: ShadowRoot = base.attachShadow ({| mode = "ouvrir" |})

    faire
        laisser clone = template.content.cloneNode (vraie)
        shadowRoot.appendChild (clone)

    laisser button = shadowRoot.querySelector ("button")

    membre this.render() =
        button.innerHTML <- this.getAttribute ("label")

    passeroutre _.connectedCallback() = printf "connected callback"

    passeroutre this.attributeChangedCallback(name, oldVal, newVal) = this.render ()

attachStaticGetter<Button, _> "observedAttributes" (fon () -> [| "label" |])

customElements.define ("my-button", jsConstructor<Button>)
