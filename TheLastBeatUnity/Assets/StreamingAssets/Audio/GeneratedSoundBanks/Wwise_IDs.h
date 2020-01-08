/////////////////////////////////////////////////////////////////////////////////////////////////////
//
// Audiokinetic Wwise generated include file. Do not edit.
//
/////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef __WWISE_IDS_H__
#define __WWISE_IDS_H__

#include <AK/SoundEngine/Common/AkTypes.h>

namespace AK
{
    namespace EVENTS
    {
        static const AkUniqueID INT_HITMUSICFX = 3191278813U;
        static const AkUniqueID PLAY_AMBDESERT = 3681445093U;
        static const AkUniqueID PLAY_BLINK = 3106918468U;
        static const AkUniqueID PLAY_HIT_ENEMY = 3889018732U;
        static const AkUniqueID PLAY_HIT_PLAYER = 2257378511U;
        static const AkUniqueID PLAY_MUSIC = 2932040671U;
        static const AkUniqueID PLAY_REWIND = 869704911U;
        static const AkUniqueID PLAY_RUSHFXOFFBEAT = 3852757989U;
        static const AkUniqueID PLAY_RUSHFXONBEAT = 2567002569U;
        static const AkUniqueID PLAY_WILDLIFEDESERT_1 = 92132223U;
        static const AkUniqueID PLAY_WILDLIFEDESERT_2 = 92132220U;
        static const AkUniqueID SETREWIND_LOOPA = 4276712656U;
        static const AkUniqueID SETREWIND_LOOPB = 4276712659U;
        static const AkUniqueID SETREWIND_LOOPC = 4276712658U;
        static const AkUniqueID STOP_ALL = 452547817U;
        static const AkUniqueID STOP_AMB = 435770000U;
        static const AkUniqueID STOP_REWIND = 564679353U;
    } // namespace EVENTS

    namespace STATES
    {
        namespace FEEDBACKLEVEL
        {
            static const AkUniqueID GROUP = 291463486U;

            namespace STATE
            {
                static const AkUniqueID LEVEL1 = 2678230382U;
                static const AkUniqueID LEVEL2 = 2678230381U;
                static const AkUniqueID LEVEL3 = 2678230380U;
            } // namespace STATE
        } // namespace FEEDBACKLEVEL

        namespace MUSIC_COMBATEXPLO_SWITCH
        {
            static const AkUniqueID GROUP = 2355547200U;

            namespace STATE
            {
                static const AkUniqueID COMBAT = 2764240573U;
                static const AkUniqueID EXPLO = 3814499265U;
            } // namespace STATE
        } // namespace MUSIC_COMBATEXPLO_SWITCH

        namespace MUSIC_CRITICLEVEL
        {
            static const AkUniqueID GROUP = 1082318473U;

            namespace STATE
            {
                static const AkUniqueID CRITIC = 2938985745U;
                static const AkUniqueID NOTCRITIC = 3922556052U;
            } // namespace STATE
        } // namespace MUSIC_CRITICLEVEL

        namespace MUSIC_LIMIT_SWITCH
        {
            static const AkUniqueID GROUP = 960036381U;

            namespace STATE
            {
                static const AkUniqueID COMBAT_CALM = 2894236345U;
                static const AkUniqueID COMBAT_LIMIT = 2902872313U;
            } // namespace STATE
        } // namespace MUSIC_LIMIT_SWITCH

        namespace MUSIC_SWITCH
        {
            static const AkUniqueID GROUP = 2724869341U;

            namespace STATE
            {
                static const AkUniqueID FINAL_COMBAT = 1993352646U;
                static const AkUniqueID LAND = 674522502U;
            } // namespace STATE
        } // namespace MUSIC_SWITCH

        namespace REWIND_COMBAT_LOOP
        {
            static const AkUniqueID GROUP = 2305855220U;

            namespace STATE
            {
                static const AkUniqueID LOOPA = 225293556U;
                static const AkUniqueID LOOPB = 225293559U;
                static const AkUniqueID LOOPC = 225293558U;
            } // namespace STATE
        } // namespace REWIND_COMBAT_LOOP

        namespace REWIND_STATE
        {
            static const AkUniqueID GROUP = 2996289134U;

            namespace STATE
            {
                static const AkUniqueID NORMAL = 1160234136U;
                static const AkUniqueID REWIND = 1673109572U;
            } // namespace STATE
        } // namespace REWIND_STATE

    } // namespace STATES

    namespace GAME_PARAMETERS
    {
        static const AkUniqueID INT_MUSICCRUSHER = 807022328U;
        static const AkUniqueID INT_SYNCDUCKING = 758808735U;
        static const AkUniqueID MUSICPOSITION = 3149782607U;
    } // namespace GAME_PARAMETERS

    namespace TRIGGERS
    {
        static const AkUniqueID ONBEAT = 1813808854U;
    } // namespace TRIGGERS

    namespace BANKS
    {
        static const AkUniqueID INIT = 1355168291U;
        static const AkUniqueID THELASTBANK = 177400472U;
    } // namespace BANKS

    namespace BUSSES
    {
        static const AkUniqueID GAMEPLAYSFX = 4065441226U;
        static const AkUniqueID MASTER_AUDIO_BUS = 3803692087U;
        static const AkUniqueID MFX = 931006280U;
        static const AkUniqueID MUSIC = 3991942870U;
        static const AkUniqueID SFX = 393239870U;
        static const AkUniqueID SYNCFEEDBACK = 176430053U;
    } // namespace BUSSES

    namespace AUX_BUSSES
    {
        static const AkUniqueID MFXREWINDVERB = 731829278U;
    } // namespace AUX_BUSSES

    namespace AUDIO_DEVICES
    {
        static const AkUniqueID NO_OUTPUT = 2317455096U;
        static const AkUniqueID SYSTEM = 3859886410U;
    } // namespace AUDIO_DEVICES

}// namespace AK

#endif // __WWISE_IDS_H__
