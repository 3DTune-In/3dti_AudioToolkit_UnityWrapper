//
//  AppDelegate.m
//  
//
//  Created by Ragnar Hrafnkelsson on 01/03/2017.
//
//

#import "AppDelegate.h"
#import "AudioPluginInterface.h"

IMPL_APP_CONTROLLER_SUBCLASS(AppDelegate)

@implementation AppDelegate

- (void)preStartUnity {
  UnityRegisterAudioPlugin( &UnityGetAudioEffectDefinitions );
}

@end
