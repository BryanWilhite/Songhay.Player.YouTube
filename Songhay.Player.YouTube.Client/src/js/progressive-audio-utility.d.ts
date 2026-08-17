/// <reference types="blazor__javascript-interop" />
import { WindowAnimation } from 'songhay';
export declare class ProgressiveAudioUtility {
    static playAnimation: WindowAnimation | null;
    static handleAudioMetadataLoadedAsync(instance: DotNet.DotNetObject, audio: HTMLAudioElement | null): Promise<void>;
    static invokeDotNetMethodAsync(instance: DotNet.DotNetObject, audio: HTMLAudioElement | null): Promise<void>;
    static loadAudioTrack(audio: HTMLAudioElement | null, src: string): void;
    static setAudioCurrentTime(input: HTMLInputElement | null, audio: HTMLAudioElement | null): void;
    static startPlayAnimationAsync(instance: DotNet.DotNetObject, audio: HTMLAudioElement | null): Promise<void>;
    static stopPlayAnimationAsync(instance: DotNet.DotNetObject, audio: HTMLAudioElement | null): Promise<void>;
}
