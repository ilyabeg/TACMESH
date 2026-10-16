import windowEvents from './windowEvents';

// general dispatcher that dispatches a custom window event flexibly
//
export default function dispatchWindowEvent<T extends keyof windowEvents>(
    method: T,
    params: windowEvents[T]
) {
    const customEvent = new CustomEvent(method, { detail: params });
    window.dispatchEvent(customEvent);
}
