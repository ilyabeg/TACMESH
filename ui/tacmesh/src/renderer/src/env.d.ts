/// <reference types="vite/client" />

import { clientPushNotification } from "../../shared/ipc/clientToServer"

// using a global decleration allows 'injecting' this interfaces
// directly into the global scope
//
declare global {

    // create an interface that ADDS the electronAPI attribute/field
    // to the Window object to be able to use the exposed API from
    // the preload script from the renderer process using the Window 
    // object (declaration merging)    
    //
    // The reason this has to be delclared and added to the Window object
    // attributes before running the project is because typescript requires
    // the type of this attribute to be present before run-time (during compilation)
    // for the renderer to access it. the preload script exposes the actual api
    // but this declaration only allows the code to access it before compiling it.
    //
    export interface Window {
        electronAPI: {

            // the exposed API function itself
            //
            pushNotification: <T extends keyof clientPushNotification>(
                method: T,
                params: clientPushNotification[T]['params']
            ) => void
        }
    }
}
