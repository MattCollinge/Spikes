//*********************************************************
//
// Copyright Microsoft Corporation
//
// Licensed under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in
// compliance with the License. You may obtain a copy of
// the License at
//
// http://www.apache.org/licenses/LICENSE-2.0 
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES
// OR CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED,
// INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES
// OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABLITY OR NON-INFRINGEMENT.
//
// See the Apache 2 License for the specific language
// governing permissions and limitations under the License.
//
//*********************************************************

namespace StreamInsight.Samples.UserExtensions.Afa
{
    using Microsoft.ComplexEventProcessing;

    /// <summary>
    /// The delegate for arcs (transitions) in an AFA.
    /// </summary>
    /// <typeparam name="TInput">The input event type.</typeparam>
    /// <typeparam name="TRegister">The register type.</typeparam>
    /// <param name="inputEvent">The input event.</param>
    /// <param name="oldRegister">The old register value.</param>
    /// <param name="newRegister">Output the new register value after transition.</param>
    /// <returns></returns>
    public delegate bool TransitionDelegate<TInput, TRegister>(PointEvent<TInput> inputEvent, TRegister oldRegister, out TRegister newRegister);
}