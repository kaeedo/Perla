import { html } from 'lit';
// if it doesn't exist it will throw an error
import { envValue, SOME_TOKEN } from '/env.js'

// if you import dinamically it will be undefined
import("/env.js").then(({ I_DONT_EXIST }) => {
    console.log(I_DONT_EXIST ?? "export not present")
})

export function TsSample(value: number, kind: Kind) {
    return html`
        <div>
            <h1 style="color: var(--primary-color)">Hello From Typescript!</h1>
            <p style="color: var(--primary-color)">The path is ${location.pathname}</p>
            <p style="color: var(--link-color)">Vaue: ${value}, Kind: ${kind}</p>
            <p style="color: var(--danger-color)">Env Vars! <b>${envValue}</b>, <b>${SOME_TOKEN}</b> </p>
        </div>
    `;
}


export type Kind = 'a' | 'b' | 'c';